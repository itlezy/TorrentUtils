using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using ILCommon;
using ILCommon.Data.Model;
using ILCommon.IO;

using MetadataDownloader.Data;

using MonoTorrent;
using MonoTorrent.Client;

using static Crayon.Output;

namespace MetadataDownloader
{
    class QueueManager
    {
        readonly MDConfig c = new MDConfig ();
        readonly DAO dao = new DAO ();
        readonly List<int> avgResponseTime = new List<int> () { 0 };
        readonly IEnumerable<string> banWords = File.ReadAllLines (new MDConfig ().BAN_WORDS_FILE).Where (m => !string.IsNullOrWhiteSpace (m));

        int timeoutCount = 0, downloadedCount = 0, skippedCount = 0;
        DateTime lastDowloaded = DateTime.Now;

        public async Task DownloadAsync (
        string hash,
        ClientEngine engine,
        CancellationToken token)
        {
            if (MagnetLink.TryParse (c.MAGNET_PREFIX + hash, out MagnetLink magnetLink)) {

                var manager = await engine.AddAsync (magnetLink, c.TMP_SAVE_DIR);
                var hashId = magnetLink.InfoHashes.V1.ToHex ().ToLower ();

                Console.WriteLine (
                    $"DownloadAsync()  Adding   torrent  {Green (hashId)}");

                await manager.StartAsync ();
                await manager.WaitForMetadataAsync (token);

                if (manager.HasMetadata && manager.Files.Count > 0) {
                    var utName = manager.Torrent.Name;
                    var utComment = manager.Torrent.Comment;
                    var elapsedms = (DateTime.Now - manager.StartTime).Milliseconds + (DateTime.Now - manager.StartTime).Seconds * 1000;

                    Console.WriteLine (
                        $"DownloadAsync()  Metadata Received {Green (hashId)} in {elapsedms:n0}ms - * [ {Magenta (utName)} ] * -");

                    avgResponseTime.Add (elapsedms);

                    try {
                        var fName = manager.Files.OrderByDescending (t => t.Length).First ().Path;
                        var fLen = manager.Files.OrderByDescending (t => t.Length).First ().Length;
                        var fileNameManager = new FileNameManager ();

                        // here I can decide if the torrent largest file already exists, then I can skip to save it

                        if (fLen < (c.TORRENT_MIN_FILE_SIZE_MB * 1024 * 1024)) {
                            skippedCount++;

                            Console.WriteLine ($"DownloadAsync()  Skipping torrent  {Yellow (hashId)} file too small [ {fName} ] {fLen:n0}");

                        } else if (
                            dao.HasBeenDownloaded (new MDownloadedFile () { FileName = fName, Length = fLen }) ||
                            dao.HasBeenDownloaded (new MDownloadedTorr () { HashId = hashId })
                            ) { //TODO maybe check also hashId when loading torrhashes
                            skippedCount++;

                            Console.WriteLine ($"DownloadAsync()  Skipping torrent  {Yellow (hashId)} already downloaded [ {fName} ] {fLen:n0}");

                        } else if (!fileNameManager.IsMostlyLatin (utName)) {
                            skippedCount++;

                            Console.WriteLine ($"DownloadAsync()  Skipping torrent  {Yellow (hashId)} non latin file [ {fName} ] {fLen:n0}");

                        } else if (fileNameManager.ContainsBanWord (utName, utComment, fName, banWords)) {
                            skippedCount++;

                            Console.WriteLine ($"DownloadAsync()  Skipping torrent  {Yellow (hashId)} ban word! [ {fName} ] {fLen:n0}");

                            // don't want those on the DB!
                            utName = "--------";
                            utComment = "--------";

                        } else {
                            downloadedCount++;
                            lastDowloaded = DateTime.Now;

                            var subCat = fileNameManager.GetSubCat (manager.Torrent.Name);
                            var targetFName = "G" + (10 * Math.Round ((double) fLen / (1024 * 1024 * 1024), 1)).ToString ().PadLeft (3, '0') + "_" +
                                fileNameManager.SafeName (manager.Torrent.Name);

                            File.Copy (manager.MetadataPath, c.TORRENT_OUTPUT_PATH + subCat + @"\" + targetFName + ".torrent");

                        }

                        dao.UpdateHashId (
                            new MTorr () {
                                HashId = hashId,
                                Name = utName,
                                Length = manager.Torrent.Size,
                                Comment = utComment,
                                Timeout = false
                            });

                    } catch (Exception ex) {
                        Console.Error.WriteLine ($"DownloadAsync()  Exception thrown  {Red (hashId)} {Red (ex.Message)}");
                    }

                } else {
                    Console.WriteLine ($"DownloadAsync()  No metadata for   {Red (hashId)} *** \\*//");
                }

                await manager.StopAsync (new TimeSpan (0, 0, c.TORRENT_STOP_TIMEOUT));

                try {
                    await engine.RemoveAsync (manager.Torrent, RemoveMode.CacheDataAndDownloadedData);
                } catch (Exception ex) {
                    Console.Error.WriteLine (ex.Message);
                }
            }
        }

        /// <summary>
        /// Main loop which attempts to download torrent metadata in a queue
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task MainLoop (CancellationToken token)
        {
            // Give an example of how settings can be modified for the engine.
            var settingBuilder = new EngineSettingsBuilder {
                // Allow the engine to automatically forward ports using upnp/nat-pmp (if a compatible router is available)
                AllowPortForwarding = true,

                // Automatically save a cache of the DHT table when all torrents are stopped.
                AutoSaveLoadDhtCache = true,

                // If a MagnetLink is used to download a torrent, the engine will try to load a copy of the metadata
                // it's cache directory. Otherwise the metadata will be downloaded and stored in the cache directory
                // so it can be reloaded later.
                AutoSaveLoadMagnetLinkMetadata = true,

                // Use a fixed port to accept incoming connections from other peers for testing purposes. Production usages should use a random port, 0, if possible.
                ListenEndPoint = new IPEndPoint (IPAddress.Any, 55123),

                // Use a fixed port for DHT communications for testing purposes. Production usages should use a random port, 0, if possible.
                DhtEndPoint = new IPEndPoint (IPAddress.Any, 55123),
            };

            using var engine = new ClientEngine (settingBuilder.ToSettings ());

            if (engine.Settings.AllowPortForwarding)
                Console.WriteLine ("MainLoop()       uPnP or NAT-PMP port mappings will be created for any ports needed by MonoTorrent");

            while (true) {
                await Task.Delay (c.MAIN_LOOP_INTERVAL);

                Console.WriteLine ("MainLoop()       Checking for torrents count {0,3} / {1,3} - Dowloaded {2,6:n0} Timedout {3,6:n0} Skipped {4,6:n0} - DHT nodes {5,4} last dld {6,4:n0}s ago, avg {7,3:n0}ms",
                    engine.Torrents.Count,
                    c.TORRENT_PARALLEL_LIMIT - 1,
                    downloadedCount,
                    timeoutCount,
                    skippedCount,
                    engine.Dht.NodeCount,
                    (DateTime.Now - lastDowloaded).Seconds + ((DateTime.Now - lastDowloaded).Minutes * 60),
                    avgResponseTime.Average ());

                if (engine.Torrents.Count < c.TORRENT_PARALLEL_LIMIT) {
                    var hash = dao.GetNextHashId ();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    DownloadAsync (hash, engine, token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                } else {
                    // FIFO logic, just remove the oldest
                    var torrent = engine.Torrents.First ();
                    var hashId = torrent.InfoHashes.V1.ToHex ().ToLower ();

                    Console.WriteLine ($"MainLoop()       Removing torrent  {Red (hashId)}");

                    dao.UpdateHashId (
                       new MTorr () {
                           HashId = hashId,
                           Name = null,
                           Length = 0,
                           Comment = null,
                           Timeout = true
                       });

                    timeoutCount++;

                    await torrent.StopAsync (new TimeSpan (0, 0, c.TORRENT_STOP_TIMEOUT));

                    try {
                        await engine.RemoveAsync (torrent, RemoveMode.CacheDataAndDownloadedData);
                    } catch (Exception ex) {
                        Console.Error.WriteLine (ex.Message);
                    }
                }

                if (token.IsCancellationRequested) {
                    await engine.StopAllAsync ();

                    if (engine.Settings.AllowPortForwarding)
                        Console.WriteLine ("MainLoop()       uPnP and NAT-PMP port mappings have been removed");

                    Console.WriteLine ("MainLoop()       Cancellation request received, exiting..");

                    break;
                }
            }
        }

        /// <summary>
        /// Reads torrent files metadata from disk and update the DB records,
        /// so to avoid duplicate downloads when we've already downloaded a torrent
        /// </summary>
        /// <param name="inputDir"></param>
        public void LoadDownloadedTorrents (string inputDir)
        {
            var ff = new IOManager ().ListDownloadedTorrents (inputDir);
            var mf = new List<MTorr> ();

            foreach (var f in ff.MDownloadedTorrs) {
                mf.Add (
                    new MTorr () {
                        HashId = f.HashId,
                        Name = f.Name,
                        Downloaded = true,
                        Length = f.Length,
                        Processed = true,
                        Timeout = false,
                        DownloadedTime = DateTime.UtcNow
                    });
            }

            new DAO ().UpdateDownloadedTorrentsStatus (mf);
        }
    }
}
