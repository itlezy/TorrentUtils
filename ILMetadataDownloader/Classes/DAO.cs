﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ILCommon.Data;
using ILCommon.Data.Model;

using SQLite;

namespace MetadataDownloader.Data
{
    class DAO : BaseDAO
    {
        private readonly MDConfig c = new MDConfig ();

        public void CreateTables ()
        {
            using (var db = new SQLiteConnection (c.SDB_URL)) {
                db.CreateTable<MTorr> ();
                db.CreateTable<MTorrLog> ();
            }

            using (var db = new SQLiteConnection (c.SDB_URL)) {

                db.Execute (
                    "CREATE INDEX \"MTorr_CountSeen_LastSeen\" on \"MTorr\" (\"IsAnnounce\" DESC, \"CountSeen\" DESC, \"LastSeen\" DESC, \"Processed\" DESC)"
                );

                db.Execute (
                    "CREATE INDEX \"MTorr_Processed\" ON \"MTorr\" (\"Processed\" ASC)"
                );
                
                db.Execute (
                    "CREATE UNIQUE INDEX \"MTorrLog_UQ_SeenAt_HashId\" on \"MTorrLog\" (\"SeenAt\" DESC, \"HashId\" ASC)"
                );

                db.Execute (
                    "CREATE INDEX \"MTorrLog_HashId\" ON \"MTorrLog\" ( \"HashId\" ASC)"
                );

                db.Execute (
                    "CREATE INDEX \"MTorrLog_SeenAt\" ON \"MTorrLog\" ( \"SeenAt\" DESC)"
                );

            }
        }

        public void CleanBanWords ()
        {
            var banWords = File.ReadAllLines (c.BAN_WORDS_FILE).Where (m => !string.IsNullOrWhiteSpace (m));

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                foreach (var banWord in banWords) {

                    var ins = db.Execute ($"UPDATE MTorr SET Processed = true, Downloaded = false, Timeout = false, Name = '--------', Comment = '--------' WHERE (Name LIKE '%{banWord}%' OR Comment LIKE '%{banWord}%')");

                    Console.WriteLine ("Cleaned \t{0} records, banWord \t{1}", ins, banWord);
                }
            }
        }

        /// <summary>
        /// Updates the torrents' status by processing local metadata torrent files, so to avoid duplicates
        /// </summary>
        /// <param name="torrs"></param>
        /// <returns></returns>
        public int UpdateDownloadedTorrentsStatus (List<MTorr> torrs)
        {
            Console.WriteLine ("UpdateDownloadedTorrentsStatus() ..");

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var ins = db.InsertAll (torrs, " OR IGNORE ");

                Console.WriteLine ("Loaded \t{0:n0} records out of \t{1:n0} ..", ins, torrs.Count);

            }

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var ins = db.UpdateAll (torrs);

                Console.WriteLine ("Updated \t{0:n0} records out of \t{1:n0} ..", ins, torrs.Count);

                return ins;
            }
        }

        internal void PrintTableStats ()
        {
            Console.WriteLine ("PrintTableStats() ..");

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var query = "";

                query = "SELECT COUNT(*) FROM MTorr";
                var tcount = db.ExecuteScalar<long> (query);

                query = "SELECT COUNT(*) FROM MTorrLog";
                var tlcount = db.ExecuteScalar<long> (query);

                query = "SELECT COUNT(*) FROM MTorr WHERE (Processed = true)";
                var pcount = db.ExecuteScalar<long> (query);

                query = "SELECT COUNT(*) FROM MTorr WHERE (Processed <> true)";
                var npcount = db.ExecuteScalar<long> (query);

                query = "SELECT COUNT(*) FROM MTorr WHERE (Downloaded = true)";
                var dcount = db.ExecuteScalar<long> (query);

                query = "SELECT COUNT(*) FROM MTorr WHERE (Timeout = true)";
                var ttcount = db.ExecuteScalar<long> (query);

                Console.WriteLine ($"Total Torrs \t{tcount:n0}\n non-processed \t{npcount:n0}\n processed \t{pcount:n0}\n timeout \t{ttcount:n0}\n downloaded \t{dcount:n0}\n log table \t{tlcount:n0}");
            }
        }

        public string GetNextHashId ()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew ();

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var query = "SELECT * FROM MTorr WHERE (Processed <> true) ORDER BY IsAnnounce DESC, CountSeen DESC, LastSeen DESC LIMIT 1";
                var mTorr = db.Query<MTorr> (query).FirstOrDefault ();

                if (c.DEBUG_MODE)
                    Console.WriteLine ("GetNextHashId()  Found Torrent {0}, countSeen {2}, processedTime {1}",
                        mTorr.HashId,
                        mTorr.ProcessedTime,
                        mTorr.CountSeen
                        );

                mTorr.Processed = true;
                mTorr.ProcessedTime = DateTime.UtcNow;

                var upds = db.Update (mTorr);

                if (c.DEBUG_MODE)
                    Console.WriteLine ("GetNextHashId()  Found Torrent {0}, updated {1} record", mTorr.HashId, upds);


                watch.Stop ();
                Console.WriteLine ("ms {0}", watch.ElapsedMilliseconds);

                return mTorr.HashId;
            }
            
        }

        public bool HasBeenDownloaded (MDownloadedTorr mDownloadedTorr)
        {
            using (var db = new SQLiteConnection (c.SDB_DLD_URL, SQLiteOpenFlags.ReadOnly)) {
                return db.ExecuteScalar<int> (
                    "SELECT COUNT(*) FROM MDownloadedTorr M WHERE (M.HashId = ?)",
                    mDownloadedTorr.HashId) > 0;
            }
        }

        public void UpdateHashId (MTorr mTorrentU)
        {
            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var mTorr = db.Query<MTorr> ("SELECT * FROM MTorr WHERE HashId = ? LIMIT 1", mTorrentU.HashId).FirstOrDefault ();

                if (c.DEBUG_MODE)
                    Console.WriteLine ("UpdateHashId()   Found Torrent {0}, countSeen {2}, processedTime {1}",
                        mTorr.HashId,
                        mTorr.ProcessedTime,
                        mTorr.CountSeen
                        );

                mTorr.Processed = true;
                mTorr.Downloaded = !mTorrentU.Timeout;
                mTorr.DownloadedTime = DateTime.UtcNow;
                mTorr.Name = mTorrentU.Name;
                mTorr.Comment = mTorrentU.Comment;
                mTorr.Length = mTorrentU.Length;
                mTorr.Timeout = mTorrentU.Timeout;
                mTorr.ProcessedTime = DateTime.UtcNow;

                var upds = db.Update (mTorr);

                if (c.DEBUG_MODE)
                    Console.WriteLine ("UpdateHashId()   Found Torrent {0}, updated {1} record", mTorr.HashId, upds);
            }
        }

        public void LoadHashesFromFile (string inputFile) //TODO: this should be split in IO and DAO
        {
            Console.WriteLine ("LoadHashesFromFile() [{0}]", inputFile);
            var lines = File.ReadAllLines (inputFile);

            Console.WriteLine ("Loaded \t{0:n0} lines from file [{1}]", lines.Length, inputFile);
            //Console.ReadLine ();

            var mTorrs = new List<MTorrLog> ();

            Console.WriteLine ("Processing lines, extracting hashes..");

            foreach (var line in lines) {
                if (line.Length < 50) { continue; }

                try {
                    var segs = line.Split (' ');
                    var dateSeen = DateTime.Parse (segs[1]);
                    var hashId = segs[4].Substring (1, 40).ToLower ();

                    // (I) 2022-05-31T20:09:38 - handleDHTAnnounceAlert "1234567898765434567540000000000000000000"               

                    mTorrs.Add (new MTorrLog () {
                        HashId = hashId,
                        SeenAt = dateSeen,
                        IsAnnounce = line.Contains ("handleDHTAnnounceAlert")
                    });

                } catch (Exception ex) {
                    Console.Error.WriteLine ("Error processing line [{0}] {1}", line, ex.Message);
                }

                // Console.WriteLine ("Date [{0}], HashId [{1}]", dateSeen, hashId);
            }

            Console.WriteLine ("Found \t{0:n0} hashes from text file..", mTorrs.Count);

            Console.WriteLine ("Insert new records to Log Table..");

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var ins = db.InsertAll (mTorrs, " OR IGNORE ");

                Console.WriteLine ("Loaded \t{0:n0} records to Log Table out of \t{1:n0} ..", ins, lines.Length);
            }

            Console.WriteLine ("Insert new records to Tor Table..");

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var ins = db.Execute (@"INSERT INTO MTorr (HashId, IsAnnounce, CountSeen, LastSeen, Processed)
                                        SELECT DISTINCT 
                                          HashId, IsAnnounce, COUNT(HashId) AS CountSeen, MAX(SeenAt) AS LastSeen, 0 as Processed
                                        FROM
                                          MTorrLog
                                        WHERE
                                          HashId NOT IN (SELECT DISTINCT HashId FROM MTorr)
                                        GROUP BY HashId
                                        ORDER BY CountSeen DESC");

                Console.WriteLine ("Loaded \t{0:n0} records to Tor Table out of \t{1:n0} ..", ins, lines.Length);
            }

            Console.WriteLine ("Updating counts and lastSeen..");

            using (var db = new SQLiteConnection (c.SDB_URL)) {
                var ins = db.Execute (@"UPDATE MTorr
                                        SET
                                            CountSeen = (SELECT COUNT(MTorrLog.HashId) AS CountSeen FROM MTorrLog WHERE MTorr.HashId = MTorrLog.HashId GROUP BY MTorrLog.HashId),
                                            LastSeen  = (SELECT   MAX(MTorrLog.SeenAt) AS LastSeen  FROM MTorrLog WHERE MTorr.HashId = MTorrLog.HashId GROUP BY MTorrLog.HashId)
                                        WHERE EXISTS (
                                            SELECT HashId FROM MTorrLog WHERE MTorrLog.HashId = MTorr.HashId
                                        )");

                Console.WriteLine ("Updated stats of \t{0:n0} records to Tor Table ..", ins);
            }
        }
    }
}
