﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILCommon.Model;

using MonoTorrent;

namespace ILCommon.IO
{
    public class IOManager
    {
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

        public ListDownloadedTorrentsRet ListDownloadedTorrents (String inputDir)
        {
            // Console.Error.WriteLine ($"Processing Input Directory [{ Green (inputDir)}], looking for files gt than 10Mb..");

            var allFiles = Directory.GetFiles (inputDir, "*.torrent", SearchOption.AllDirectories);
            var r = new ListDownloadedTorrentsRet ();

            for (var i = 0; i < allFiles.Length; i++) {
                try {
                    var file = new FileInfo (allFiles[i]);

                    if (file.Length > 1024) {
                        try {
                            var torr = Torrent.Load (file.FullName);

                            r.MDownloadedTorrs.Add (new MDownloadedTorr () {
                                HashId = torr.InfoHashes.V1OrV2.ToHex ().ToLower (),
                                Name = !String.IsNullOrWhiteSpace (torr.Name) ?
                                new FileNameManager ().NormalizeFileName (torr.Name) :
                                new FileNameManager ().NormalizeFileName (Path.GetFileNameWithoutExtension (file.Name)),
                                Length = torr.Size
                            });

                            var fName = torr.Files.OrderByDescending (t => t.Length).First ().Path;
                            var fLen = torr.Files.OrderByDescending (t => t.Length).First ().Length;

                            Console.WriteLine ("Torr {0} \t{1} \tfile {2}",
                                torr.InfoHashes.V1OrV2.ToHex ().ToLower (),
                                fLen,
                                fName);

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

        public List<MDownloadedFile> ListDownloadedFiles (String inputDir)
        {
            // generate the index of files + "|" + size, so to skip dups of files we have no torrent for
            const int MB_10 = 1024 * 1024 * 10;

            // Console.Error.WriteLine ($"Processing Input Directory [{ Green (inputDir)}], looking for files gt than 10Mb..");

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
