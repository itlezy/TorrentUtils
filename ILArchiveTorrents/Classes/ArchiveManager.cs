using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILCommon;
using ILCommon.IO;
using ILCommon.Model;

using MonoTorrent;

using static Crayon.Output;

namespace ArchiveTorrents
{
    class ArchiveManager
    {
        ATConfig c = new ATConfig ();
        DAO dao = new DAO ();

        /// <summary>
        /// Removes duplicate torrent files by checking the tables of downloaded torrents, downloaded files and the directory archive just in case
        /// </summary>
        public void RemDupsAndArchive ()
        {
            Console.WriteLine ($"Processing Input Directory [{ Green (c.TORR_INPUT_DIR)}], looking for duplicates..");

            // find files that have been downloaded multiple times, and delete them like "abc (1).torrent"
            var dupTorrFiles = Directory.GetFiles (c.TORR_INPUT_DIR, "* (?).torrent");

            for (var i = 0; i < dupTorrFiles.Length; i++) {
                var torrFile = new FileInfo (dupTorrFiles[i]);

                Console.WriteLine ($"Found multiple-download file {Red (torrFile.Name)}");

                File.Delete (torrFile.FullName);
            }

            Console.WriteLine ();
            Console.WriteLine ($"Processing Input Directory [{ Green (c.TORR_INPUT_DIR)}], processing torrent files..");
            Console.WriteLine ();

            // find actual torrent files
            var torrFiles = Directory.GetFiles (c.TORR_INPUT_DIR, c.TORR_EXT_WILDCARD);
            var duplicatesCount = 0;
            var copiedCount = 0;
            var totalFiles = torrFiles.Length;

            for (var i = 0; i < torrFiles.Length; i++) {
                var torrFile = new FileInfo (torrFiles[i]);

                //Console.WriteLine ($"Found file        [{ Magenta (torrFile.Name) }]");

                var torrTorr = Torrent.Load (torrFile.FullName);
                var torrLargestFile = torrTorr.Files.OrderByDescending (t => t.Length).First ();
                var torrHashId = torrTorr.InfoHashes.V1OrV2.ToHex ().ToLower ();
                var normalizedName = new FileNameManager ().NormalizeFileName (Path.GetFileNameWithoutExtension (torrFile.Name));

                Console.WriteLine ($"Found file        [{ Magenta (torrFile.Name) }], hashId { Green (torrHashId) }");
                Console.WriteLine ($"             >    [{ Magenta (torrLargestFile.Path) }], size { Green (torrLargestFile.Length.ToString ()) } ");

                if (dao.HasBeenDownloaded (torrHashId)) {
                    // remove duplicate if the same hashId was already in the list
                    Console.WriteLine ($"Duplicate found L [{ Red (torrFile.Name) }], removing..");
                    duplicatesCount++;
                } else if (dao.HasBeenDownloaded (new MDownloadedFile () { FileName = torrLargestFile.Path, Length = torrLargestFile.Length })) {
                    // remove duplicate if the same file with the same exact length was already in the list
                    Console.WriteLine ($"Duplicate found R [{ Red (torrFile.Name) }], removing..");
                    duplicatesCount++;
                } else if (
                    Directory.GetFiles (c.TORR_ARCHIVE_DIR, normalizedName + c.TORR_EXT_WILDCARD).Length > 0 ||
                    Directory.GetFiles (c.TORR_ARCHIVE_DIR_OLD, normalizedName + c.TORR_EXT_WILDCARD).Length > 0
                    ) {
                    // remove duplicate if the same torrent file exists (redundant really at this stage)
                    Console.WriteLine ($"Duplicate found F [{ Red (torrFile.Name) }], removing..");
                    duplicatesCount++;
                } else {
                    Console.WriteLine ($"Archiving torrent [{ Green (torrFile.Name) }]");

                    // archive as copy
                    File.Copy (
                                torrFile.FullName,
                                c.TORR_ARCHIVE_DIR + torrFile.Name
                                );

                    // copy to incoming folder of torrent client to pick up
                    File.Copy (
                                torrFile.FullName,
                                c.TORR_INCOMING_DIR + torrFile.Name
                                );

                    // add the hashId to the list, so to be sure we can detect duplicates even if the file-name differs
                    File.AppendAllLines (c.TORR_ARCHIVE_REG, new String[] { torrHashId });
                    // add the largest file name and size to the list, so to be sure we can detect duplicates even if the file-name differs or it's the same file in different torrent files
                    File.AppendAllLines (c.TORR_ARCHIVE_FILES_REG, new String[] { torrLargestFile.Path + "|" + torrLargestFile.Length });

                    dao.LoadDownloadedFiles (
                        new List<MDownloadedFile> () {
                            new MDownloadedFile () { FileName = torrLargestFile.Path, Length = torrLargestFile.Length } });

                    dao.LoadDownloadedTorrents (
                        new List<MDownloadedTorr> () {
                            new MDownloadedTorr () {
                                HashId = torrHashId,
                                Length = torrTorr.Size,
                                Name = !String.IsNullOrWhiteSpace (torrTorr.Name) ?
                                    new FileNameManager ().NormalizeFileName (torrTorr.Name) :
                                    new FileNameManager ().NormalizeFileName (Path.GetFileNameWithoutExtension (torrFile.Name))
                            } });

                    copiedCount++;
                }

                // delete original file at the end
                File.Delete (torrFile.FullName);
                Console.WriteLine ();
            }

            Console.WriteLine ();
            Console.WriteLine ($"{Green ("It's all good man.") } Duplicates { duplicatesCount }, copied { copiedCount } out of { totalFiles } ");

            //Thread.Sleep (4000);
            Console.ReadLine ();
        }

        /// <summary>
        /// Loads the table of downloaded files (name, size) by scanning a local directory
        /// </summary>
        /// <param name="inputDir"></param>
        public void LoadDownloadedFiles (String inputDir)
        {
            var ff = new IOManager ().ListDownloadedFiles (inputDir);

            var ins = dao.LoadDownloadedFiles (ff);

            Console.WriteLine ("Loaded {0} MDownloadedFiles records out of {1} ..", ins, ff.Count);
        }

        /// <summary>
        /// Loads the table of downloaded torrents (hashId) *and* the table of downloaded files, based on the torrent metadata
        /// </summary>
        /// <param name="inputDir"></param>
        public void LoadDownloadedTorrents (String inputDir)
        {
            var ins = 0;
            var ff = new IOManager ().ListDownloadedTorrents (inputDir);

            ins = dao.LoadDownloadedTorrents (ff.MDownloadedTorrs);

            Console.WriteLine ("Loaded {0} MDownloadedTorrs records out of {1} ..", ins, ff.MDownloadedTorrs.Count);

            ins = dao.LoadDownloadedFiles (ff.MDownloadedFiles);

            Console.WriteLine ("Loaded {0} MDownloadedFiles records out of {1} ..", ins, ff.MDownloadedFiles.Count);

        }

    }
}
