using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Alphaleonis.Win32.Filesystem;

using ILCommon;
using ILCommon.Data.Model;
using ILCommon.IO;

using MonoTorrent;

using static Crayon.Output;

namespace ArchiveTorrents
{
    class ArchiveManager
    {
        readonly ATConfig c = new ATConfig ();
        readonly DAO dao = new DAO ();

        public void RemDupsAndArchive (bool skipCopyToIncoming, string inputDir)
        {
            int duplicatesCount = 0, copiedCount = 0, totalFiles = 0;

            RemDupsAndArchiveTorrents (inputDir, skipCopyToIncoming, ref duplicatesCount, ref copiedCount, ref totalFiles);

            RemDupsAndArchiveHashes (inputDir, ref duplicatesCount, ref copiedCount, ref totalFiles);

            Console.WriteLine ();

            Console.WriteLine ($"{Green ("It's all good man.") } Duplicates { duplicatesCount }, copied { copiedCount } out of { totalFiles } ");
        }

        /// <summary>
        /// Removes duplicate torrent files by checking the tables of downloaded torrents, downloaded files and the directory archive just in case
        /// </summary>
        public void RemDupsAndArchive (bool skipCopyToIncoming)
        {
            RemDupsAndArchive (skipCopyToIncoming, c.TORR_INPUT_DIR);
        }

        internal void CacheDownloadedTorrents (string inputDir)
        {
            var ff = new IOManager ().ListDownloadedTorrents (inputDir, string.Empty);

            foreach (var f in ff.MDownloadedTorrs) {
                using (TorrageWS.CachePortTypeClient c = new TorrageWS.CachePortTypeClient (
                    new System.ServiceModel.BasicHttpsBinding (),
                    new System.ServiceModel.EndpointAddress ("https://itorrents.org/api/torrage.wsdl"))) {

                    Console.WriteLine ("Loading {0}", f.HashId, f.Name);
                    c.cacheTorrent (Convert.ToBase64String (File.ReadAllBytes (f.FullName)));
                    Console.WriteLine ("OK");
                }

                Thread.Sleep (3000);
            }
        }

        private bool Matches (string[] inputValues, IEnumerable<string> matchers)
        {
            foreach (var inputValue in inputValues) {
                foreach (var what in matchers) {
                    if (!string.IsNullOrWhiteSpace (inputValue) &&
                        inputValue.IndexOf (what, StringComparison.InvariantCultureIgnoreCase) > 0)
                        return true;
                }
            }

            return false;
        }

        const int BUF_SIZE = 1204 * 1024 * 4;

        internal void DeleteNullFiles (string inputDir)
        {
            var allFiles = Directory.GetFiles (inputDir, "*.*", System.IO.SearchOption.AllDirectories);
            var bin = new byte[BUF_SIZE];

            for (var i = 0; i < allFiles.Length; i++) {
                try {
                    var file = new FileInfo (allFiles[i]);

                    var fs = File.OpenRead (file.FullName);
                    var foundNN = false;

                    for (
                        int bs = fs.Read (bin, 0, BUF_SIZE), maxc = 0;
                        bs > 0 && maxc < 33 && !foundNN;
                        maxc++
                        ) {

                        for (int r = 0; r < bs; r++) {

                            if (bin[r] != 0x0) {
                                foundNN = true;
                                break;
                            }
                        }
                    }

                    if (!foundNN)
                        Console.WriteLine ("DEL \"{0}\"", file.FullName);
                    //else
                    //   Console.WriteLine ("Not null file   {0}", file.FullName);

                } catch (System.IO.FileNotFoundException ex) {
                    Console.Error.WriteLine ("File name too long {0}", ex.Message);
                }
            }

        }

        /// <summary>
        /// Determines where to archive the torrent file based on matching rules
        /// </summary>
        private string MapArchiveDir (Torrent torrTorr, string defaultDir)
        {
            if (string.IsNullOrWhiteSpace (c.TORR_ARCHIVE_DIR_CONFIG) ||
                c.TORR_ARCHIVE_DIR_CONFIG.IndexOf ("|") <= 0 ||
                c.TORR_ARCHIVE_DIR_CONFIG.IndexOf ("/") <= 0)
                return defaultDir;

            var rules = c.TORR_ARCHIVE_DIR_CONFIG.Split ('|').Select (s => s.Trim ());

            foreach (var rule in rules) {
                if (string.IsNullOrWhiteSpace (rule) || rule.IndexOf ("/") <= 0)
                    continue;

                var matchers = rule.Split ('/')[0].Split (';').Select (s => s.Trim ());
                var targetDir = rule.Split ('/')[1].Trim ();

                if (
                    Matches (new string[]{
                        torrTorr.Name,
                        torrTorr.Comment,
                        torrTorr.CreatedBy,
                        torrTorr.Source
                    },
                    matchers
                )) {
                    return targetDir;
                }
            }

            return defaultDir;
        }

        /// <summary>
        /// Removes duplicates and archives torrent files
        /// </summary>
        /// <param name="skipCopyToIncoming">Just archive, do not copy to incoming BT client dir</param>
        private void RemDupsAndArchiveTorrents (string inputDir, bool skipCopyToIncoming, ref int duplicatesCount, ref int copiedCount, ref int totalFiles)
        {
            Console.WriteLine ($"Processing Input Directory [{ Green (inputDir)}], looking for duplicates..");

            // find files that have been downloaded multiple times, and delete them like "abc (1).torrent"
            var dupTorrFiles = Directory.GetFiles (inputDir, "* (?).torrent");

            for (var i = 0; i < dupTorrFiles.Length; i++) {
                var torrFile = new FileInfo (dupTorrFiles[i]);

                Console.WriteLine ($"Found multiple-download file {Red (torrFile.Name)}");

                File.Delete (torrFile.FullName);
            }

            Console.WriteLine ();
            Console.WriteLine ($"Processing Input Directory [{ Green (inputDir)}], processing torrent files..");
            Console.WriteLine ();

            // find actual torrent files
            var torrFiles = Directory.GetFiles (inputDir, c.TORR_EXT_WILDCARD);
            totalFiles = torrFiles.Length;

            for (var i = 0; i < totalFiles; i++) {
                var torrFile = new FileInfo (torrFiles[i]);

                Torrent torrTorr;
                try {
                    torrTorr = Torrent.Load (File.ReadAllBytes (torrFile.FullName));
                } catch (Exception ex) {
                    Console.Error.WriteLine ($"Invalid file      [{ Magenta (torrFile.Name) }] [ { Red (ex.Message) }]");
                    File.Move (torrFile.FullName, torrFile.FullName + "_" + Guid.NewGuid () + ".invalid");
                    continue;
                }

                var torrLargestFile = torrTorr.Files.OrderByDescending (t => t.Length).First ();
                var torrHashId = torrTorr.InfoHashes.V1OrV2.ToHex ().ToLower ();
                var normalizedFileName = Path.GetFileNameWithoutExtension (torrFile.Name);
                var normalizedTorrName = new FileNameManager ().SafeName (torrTorr.Name);

                var isDuplicate = false;
                var forceArchive = false;

                Console.WriteLine ($"Found file        [{ Magenta (torrFile.Name) }], hashId { Green (torrHashId) }, total size { Green (torrTorr.Size.ToString ("n0")) }");
                Console.WriteLine ($"             >    [{ Magenta (torrLargestFile.Path) }], size { Green (torrLargestFile.Length.ToString ("n0")) } ");

                if (dao.HasBeenDownloaded (torrHashId)) {
                    // remove duplicate if the same hashId was already in the list
                    Console.WriteLine ($"Duplicate found L [{ Red (torrFile.Name) }], removing..");
                    duplicatesCount++;
                    isDuplicate = true;
                } else if (dao.HasBeenDownloaded (new MDownloadedFile () { FileName = torrLargestFile.Path, Length = torrLargestFile.Length })) {
                    // remove duplicate if the same file with the same exact length was already in the list
                    Console.WriteLine ($"Duplicate found R [{ Red (torrFile.Name) }], removing..");
                    duplicatesCount++;
                    isDuplicate = true;
                } else if (
                    Directory.GetFiles (c.TORR_ARCHIVE_DIR, normalizedFileName + c.TORR_EXT_WILDCARD).Length > 0 ||
                    Directory.GetFiles (c.TORR_ARCHIVE_DIR, normalizedTorrName + c.TORR_EXT_WILDCARD).Length > 0) {
                    // remove duplicate if the same torrent file exists
                    Console.WriteLine ($"Duplicate found F [{ Red (torrFile.Name) }], removing..");
                    duplicatesCount++;
                    isDuplicate = true;
                } else if (
                    Directory.GetFiles (c.TORR_ARCHIVE_DIR_OLD, normalizedFileName + c.TORR_EXT_WILDCARD).Length > 0 ||
                    Directory.GetFiles (c.TORR_ARCHIVE_DIR_OLD, normalizedTorrName + c.TORR_EXT_WILDCARD).Length > 0) {
                    // remove duplicate if the same torrent file exists
                    Console.WriteLine ($"Duplicate found Z [{ Red (torrFile.Name) }], removing..");
                    duplicatesCount++;
                    isDuplicate = true;
                    forceArchive = true;
                }

                // execute all the time, but..
                {
                    // 1. archive the torrent file, by copying it
                    if (!File.Exists (c.TORR_ARCHIVE_DIR + torrFile.Name) || forceArchive) {

                        // archive as copy
                        File.Copy (
                                    torrFile.FullName,
                                    MapArchiveDir (torrTorr, c.TORR_ARCHIVE_DIR) + normalizedTorrName + ".torrent",
                                    true
                                    );
                    }

                    // 2. copy to incoming folder of torrent client to pick up
                    if (!skipCopyToIncoming && !isDuplicate) {
                        Console.WriteLine ($"Archiving torrent [{ Green (torrFile.Name) }]");

                        File.Copy (
                                    torrFile.FullName,
                                    c.TORR_INCOMING_DIR + normalizedTorrName + ".torrent",
                                    true
                                    );

                        copiedCount++;
                    }

                    if (!isDuplicate) {
                        // legacy : update the torrent download status to the file-list
                        // add the hashId to the list, so to be sure we can detect duplicates even if the file-name differs
                        File.AppendAllLines (c.TORR_ARCHIVE_REG, new string[] { torrHashId });
                        // add the largest file name and size to the list, so to be sure we can detect duplicates even if the file-name differs or it's the same file in different torrent files
                        File.AppendAllLines (c.TORR_ARCHIVE_FILES_REG, new string[] { torrLargestFile.Path + "|" + torrLargestFile.Length });
                    }

                    // update the torrent download status to the DB
                    dao.LoadDownloadedFiles (
                        new List<MDownloadedFile> () {
                            new MDownloadedFile () { FileName = torrLargestFile.Path, Length = torrLargestFile.Length } });

                    dao.LoadDownloadedTorrents (
                        new List<MDownloadedTorr> () {
                            new MDownloadedTorr () {
                                HashId = torrHashId,
                                Length = torrTorr.Size,
                                Name = !string.IsNullOrWhiteSpace (torrTorr.Name) ?
                                    torrTorr.Name :
                                    Path.GetFileNameWithoutExtension (torrFile.Name)
                            } });

                }

                // delete original file at the end
                File.Delete (torrFile.FullName);
                Console.WriteLine ();
            }
        }

        /// <summary>
        /// Processes the .torrhash files that are hashId files (the file name is the hashId)
        /// </summary>
        private void RemDupsAndArchiveHashes (string inputDir, ref int duplicatesCount, ref int copiedCount, ref int totalFiles)
        {
            // process hash files that are only hasId.torrhash
            Console.WriteLine ();
            Console.WriteLine ($"Processing Input Directory [{ Green (inputDir)}], processing hash files..");
            Console.WriteLine ();

            var dts = DateTime.Now.ToString ("yyyyMMddHHmmss");
            var torrHashFiles = Directory.GetFiles (inputDir, "*.txt").
                Where (fileName => Regex.IsMatch (fileName, ILCommon.Config.Constants.REGEX_SHA, RegexOptions.IgnoreCase)).ToArray ();

            totalFiles += torrHashFiles.Length;

            for (var i = 0; i < torrHashFiles.Length; i++) {
                var torrHashFile = new FileInfo (torrHashFiles[i]);

                //Console.WriteLine ($"Found file        [{ Magenta (torrFile.Name) }]");

                var torrHashId = Path.GetFileNameWithoutExtension (torrHashFile.Name).ToLower ();

                Console.WriteLine ($"Found file        [{ Magenta (torrHashFile.Name) }], hashId { Green (torrHashId) }");

                if (dao.HasBeenDownloaded (torrHashId)) {
                    // remove duplicate if the same hashId was already in the list
                    Console.WriteLine ($"Duplicate found L [{ Red (torrHashFile.Name) }], removing..");
                    duplicatesCount++;
                } else {
                    Console.WriteLine ($"Archiving torrent [{ Green (torrHashFile.Name) }]");

                    File.AppendAllLines (inputDir + Path.DirectorySeparator + "dld_hashIds_" + dts + ".txt", new string[] { torrHashId });

                    // add the hashId to the list, so to be sure we can detect duplicates even if the file-name differs
                    File.AppendAllLines (c.TORR_ARCHIVE_REG, new string[] { torrHashId });

                    dao.LoadDownloadedTorrents (
                        new List<MDownloadedTorr> () {
                            new MDownloadedTorr () {
                                HashId = torrHashId,
                                Length = -1,
                                Name = torrHashId
                            } });

                    copiedCount++;
                }

                // delete original file at the end
                File.Delete (torrHashFile.FullName);
                Console.WriteLine ();
            }
        }

        /// <summary>
        /// Loads the table of downloaded files (name, size) by scanning a local directory
        /// </summary>
        /// <param name="inputDir"></param>
        public void LoadDownloadedFiles (string inputDir)
        {
            var ff = new IOManager ().ListDownloadedFiles (inputDir);

            var ins = dao.LoadDownloadedFiles (ff);

            Console.WriteLine ("Loaded \t{0:n0} MDownloadedFiles records out of \t{1:n0} ..", ins, ff.Count);
        }

        /// <summary>
        /// Loads the table of downloaded torrents (hashId) *and* the table of downloaded files, based on the torrent metadata
        /// </summary>
        /// <param name="inputDir"></param>
        public void LoadDownloadedTorrents (string inputDir, string fileExtension)
        {
            var ins = 0;
            var ff = new IOManager ().ListDownloadedTorrents (inputDir, fileExtension);

            ins = dao.LoadDownloadedTorrents (ff.MDownloadedTorrs);

            Console.WriteLine ("Loaded \t{0:n0} MDownloadedTorrs records out of \t{1:n0} ..", ins, ff.MDownloadedTorrs.Count);

            ins = dao.LoadDownloadedFiles (ff.MDownloadedFiles);

            Console.WriteLine ("Loaded \t{0:n0} MDownloadedFiles records out of \t{1:n0} ..", ins, ff.MDownloadedFiles.Count);

        }

        /// <summary>
        /// Sync torrents being in use by a BT client to the archive directory, so to ensure to have all copies archived
        /// </summary>
        /// <param name="inputDir"></param>
        public void SyncDownloadedTorrents (string inputDir, string fileExtension)
        {
            var ff = new IOManager ().ListDownloadedTorrents (inputDir, fileExtension);

            foreach (var f in ff.MDownloadedTorrs) {
                // just use a platform specific safe name for the file system, the normalization will happen at the RemDupsAndArchive step
                var safeName = new FileNameManager ().SafeName (f.Name).Replace (".torrent", "");
                var targetName = c.TORR_INPUT_DIR + Path.DirectorySeparator + safeName + ".torrent";

                Console.WriteLine ($"Processing file   [{ Magenta (f.FullName) }]\n             >    [{ White (targetName) }]");

                if (Regex.IsMatch (safeName, ILCommon.Config.Constants.REGEX_SHA, RegexOptions.IgnoreCase)) {
                    Console.WriteLine ($"Skipping metadata [{ Red (targetName) }]\n");
                } else if (!File.Exists (targetName)
                      &&
                      // also check if there's a match of the safeName (which is stripped of web-site markers)
                      Directory.GetFiles (c.TORR_INPUT_DIR, safeName + c.TORR_EXT_WILDCARD, System.IO.SearchOption.TopDirectoryOnly).Length == 0) {
                    try {
                        File.Copy (f.FullName, targetName);
                        Console.WriteLine ($"Copying to        [{ Green (targetName) }]\n");

                    } catch (Exception ex) {
                        Console.Error.WriteLine ("Unable to copy file '{0}', to '{1}' - {2}", f.FullName, targetName, ex.Message);
                    }

                }
            }
        }
    }
}
