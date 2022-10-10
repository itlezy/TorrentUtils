using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ILCommon.Data.Model;

using MonoTorrent;

namespace ILCommon.IO
{
    public class IOManager
    {
        readonly Config.CommonConfig c = new Config.CommonConfig ();

        public class ListDownloadedTorrentsRet
        {
            public List<MDownloadedTorr> MDownloadedTorrs { get; }
            public List<MDownloadedFile> MDownloadedFiles { get; }

            public ListDownloadedTorrentsRet ()
            {
                MDownloadedTorrs = new List<MDownloadedTorr> ();
                MDownloadedFiles = new List<MDownloadedFile> ();
            }
        }

        public ListDownloadedTorrentsRet ListDownloadedTorrents (string inputDir, string fileExtension)
        {
            string fExt = string.IsNullOrWhiteSpace (fileExtension) ? c.TORR_EXT_WILDCARD : fileExtension;

            var allFiles = Directory.GetFiles (inputDir, fExt, SearchOption.AllDirectories);

            if (c.DEBUG_MODE)
                Console.WriteLine ("Found {0} files in dir '{1}', ext '{2}'", allFiles.Length, inputDir, fExt);

            var r = new ListDownloadedTorrentsRet ();

            for (var i = 0; i < allFiles.Length; i++) {
                try {
                    var file = new FileInfo (allFiles[i]);

                    if (file.Length > 1024) {
                        try {
                            var torr = Torrent.Load (file.FullName);

                            r.MDownloadedTorrs.Add (new MDownloadedTorr () {
                                HashId = torr.InfoHashes.V1OrV2.ToHex ().ToLower (),
                                Name = !string.IsNullOrWhiteSpace (torr.Name) ?
                                new FileNameManager ().NormalizeFileName (torr.Name) :
                                new FileNameManager ().NormalizeFileName (Path.GetFileNameWithoutExtension (file.Name)),
                                Length = torr.Size,
                                FullName = file.FullName
                            });

                            var fName = torr.Files.OrderByDescending (t => t.Length).First ().Path;
                            var fLen = torr.Files.OrderByDescending (t => t.Length).First ().Length;

                            if (c.DEBUG_MODE)
                                Console.WriteLine ("Torr {0} \t{1:n0} \tfile {2}", torr.InfoHashes.V1OrV2.ToHex ().ToLower (), fLen, fName);

                            r.MDownloadedFiles.Add (new MDownloadedFile () {
                                FileName = fName,
                                Length = fLen
                            });

                        } catch (MonoTorrent.TorrentException mex) {
                            Console.Error.WriteLine ("Error processing file (MonoTorrent.TorrentException) {0} {1}", allFiles[i], mex.Message);
                        } catch (Exception ex) {
                            Console.Error.WriteLine ("Error processing file (Exception) {0} {1}", allFiles[i], ex.Message);
                        }
                    }

                } catch (Exception ex) {
                    Console.Error.WriteLine ("File name too long {0} {1}", allFiles[i], ex.Message);
                }
            }

            return r;
        }

        /// <summary>
        /// Search if any file contains ban words and prints out the command to delete them
        /// </summary>
        /// <param name="inputDir"></param>
        public void CleanBanWords (string inputDir)
        {
            var banWords = File.ReadAllLines (c.BAN_WORDS_FILE).Where (m => !string.IsNullOrWhiteSpace (m));

            var allFiles = Directory.GetFiles (inputDir, "*.*", SearchOption.AllDirectories);

            for (var i = 0; i < allFiles.Length; i++) {
                try {
                    var file = new FileInfo (allFiles[i]);

                    foreach (var banWord in banWords) {

                        if (file.Name.IndexOf (banWord, StringComparison.InvariantCultureIgnoreCase) >= 0) {
                            Console.Error.WriteLine ("Found ban file :: DEL \"{0}\"", file.FullName);
                        }
                    }
                } catch (System.IO.FileNotFoundException ex) {
                    Console.Error.WriteLine ("File name too long {0}", ex.Message);
                }
            }
        }

        public void CleanDirs (string[] dirs)
        {
            foreach (var dir in dirs) {
                try {
                    if (Directory.Exists (dir))
                        Directory.Delete (dir, true);
                } catch (Exception ex) {
                    Console.Error.WriteLine ($"Error deleting dir {dir} - {ex.Message}");
                }
            }
        }

        public List<MDownloadedFile> ListDownloadedFiles (string inputDir)
        {
            // generate the index of files + "|" + size, so to skip dups of files we have no torrent for
            const int MB_10 = 1024 * 1024 * 10;

            if (c.DEBUG_MODE)
                Console.Error.WriteLine ($"Processing Input Directory [{inputDir}], looking for files gt than 10Mb..");

            var allFiles = Directory.GetFiles (inputDir, "*.*", SearchOption.AllDirectories);
            var mFiles = new List<MDownloadedFile> ();

            for (var i = 0; i < allFiles.Length; i++) {
                try {
                    var file = new FileInfo (allFiles[i]);

                    if (file.Length > MB_10 &&
                        !file.Name.EndsWith ("!qB")) {

                        mFiles.Add (new MDownloadedFile () { FileName = file.Name, Length = file.Length });
                    }

                } catch (System.IO.FileNotFoundException ex) {
                    Console.Error.WriteLine ("File name too long {0}", ex.Message);
                }
            }

            return mFiles;
        }
    }
}
