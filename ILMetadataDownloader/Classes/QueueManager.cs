using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using ILCommon;
using ILCommon.IO;
using ILCommon.Model;

using MonoTorrent;
using MonoTorrent.Client;

using static Crayon.Output;

namespace MetadataDownloader
{
    class QueueManager
    {
        MDConfig ac = new MDConfig ();
        DAO dao = new DAO ();
        int timeoutCount = 0, downloadedCount = 0, skippedCount = 0;
        DateTime lastDowloaded = DateTime.Now;

        public async Task DownloadAsync (
            String hash,
            ClientEngine engine,
            CancellationToken token)
        {
            if (MagnetLink.TryParse (ac.MAGNET_PREFIX + hash, out MagnetLink magnetLink)) {

                var manager = await engine.AddAsync (magnetLink, ac.TMP_SAVE_DIR);

                Console.WriteLine (
                    $"DownloadAsync()  Adding   torrent  {Green (magnetLink.InfoHashes.V1.ToHex ().ToLower ())}");

                await manager.StartAsync ();
                await manager.WaitForMetadataAsync (token);

                if (manager.HasMetadata && manager.Files.Count > 0) {
                    Console.WriteLine (
                        $"DownloadAsync()  Metadata Received {Green (magnetLink.InfoHashes.V1.ToHex ().ToLower ())} - * [ {Magenta (manager.Torrent.Name)} ] * -");


                    //manager.Files.OrderByDescending (t => t.Length).First ().FullPath

                    dao.UpdateHashId (
                        new MTorr () {
                            HashId = magnetLink.InfoHashes.V1.ToHex ().ToLower (),
                            Name = manager.Torrent.Name,
                            Length = manager.Torrent.Size,
                            Comment = manager.Torrent.Comment,
                            Timeout = false
                        });

                    try {
                        var fName = manager.Files.OrderByDescending (t => t.Length).First ().Path;
                        var fLen = manager.Files.OrderByDescending (t => t.Length).First ().Length;

                        // here I can decide if the torrent largest file already exists, then I can skip to save it

                        if (fLen < (512 * 1024 * 1024)) {
                            skippedCount++;

                            Console.WriteLine ($"DownloadAsync()  Skipping torrent  {Yellow (magnetLink.InfoHashes.V1.ToHex ().ToLower ())}, file too small [ {fName} ], {fLen}");

                        } else if (dao.HasBeenDownloaded (new MDownloadedFile () { FileName = fName, Length = fLen })) {
                            skippedCount++;

                            Console.WriteLine ($"DownloadAsync()  Skipping torrent  {Yellow (magnetLink.InfoHashes.V1.ToHex ().ToLower ())}, already downloaded [ {fName} ], {fLen}");

                        } else if (!new FileNameManager ().IsMostlyLatin (manager.Torrent.Name)) {
                            skippedCount++;

                            Console.WriteLine ($"DownloadAsync()  Skipping torrent  {Yellow (magnetLink.InfoHashes.V1.ToHex ().ToLower ())}, non latin file [ {fName} ], {fLen}");

                        } else {
                            downloadedCount++;
                            lastDowloaded = DateTime.Now;

                            var subCat = new FileNameManager ().GetSubCat (manager.Torrent.Name);

                            File.Copy (manager.MetadataPath,
                                ac.TORRENT_OUTPUT_PATH + subCat + @"\" +
                                "G" + (10 * Math.Round ((double) fLen / (1024 * 1024 * 1024), 1)) + "_" +
                                manager.Torrent.Name + ".torrent");

                        }
                    } catch (Exception ex) {
                        Console.Error.WriteLine (ex.Message);
                    }

                }

                await manager.StopAsync (new TimeSpan (0, 0, ac.TORRENT_STOP_TIMEOUT));

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
                await Task.Delay (ac.MAIN_LOOP_INTERVAL);

                Console.WriteLine ("MainLoop()       Checking for torrents count {0} / {1} - Dowloaded {2}, Timedout {3}, Skipped {4} - DHT nodes {5}, last Downloaded at {6}",
                    engine.Torrents.Count,
                    ac.TORRENT_PARALLEL_LIMIT - 1,
                    downloadedCount,
                    timeoutCount,
                    skippedCount,
                    engine.Dht.NodeCount,
                    lastDowloaded.ToLongTimeString()
                    );

                if (engine.Torrents.Count < ac.TORRENT_PARALLEL_LIMIT) {
                    var hash = dao.GetNextHashId ();

                    DownloadAsync (hash, engine, token);
                } else {
                    // FIFO logic, just remove the oldest
                    var torrent = engine.Torrents.First ();
                    Console.WriteLine ($"MainLoop()       Removing torrent  {Red (torrent.InfoHashes.V1.ToHex ().ToLower ())}");

                    dao.UpdateHashId (
                       new MTorr () {
                           HashId = torrent.InfoHashes.V1.ToHex ().ToLower (),
                           Name = null,
                           Length = 0,
                           Comment = null,
                           Timeout = true
                       });

                    timeoutCount++;

                    await torrent.StopAsync (new TimeSpan (0, 0, ac.TORRENT_STOP_TIMEOUT));

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
        public void LoadDownloadedTorrents (String inputDir)
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
