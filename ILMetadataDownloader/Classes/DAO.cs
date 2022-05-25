using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILCommon.Model;

using SQLite;

namespace MetadataDownloader
{
    class DAO
    {
        private MDConfig ac = new MDConfig ();

        public void CreateTables ()
        {
            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                db.CreateTable<MTorrLog> ();
                db.CreateTable<MTorr> ();

            }

            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var ins = db.Execute (
                    "CREATE UNIQUE INDEX \"MTorrLog_UQ\" ON \"MTorrLog\" ( \"HashId\"    ASC, \"SeenAt\"    ASC )"
                );

            }

        }

        /// <summary>
        /// Updates the torrents' status by processing local metadata torrent files, so to avoid duplicates
        /// </summary>
        /// <param name="torrs"></param>
        /// <returns></returns>
        public int UpdateDownloadedTorrentsStatus (List<MTorr> torrs)
        {
            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var ins = db.InsertAll (torrs, " OR IGNORE ");

                Console.WriteLine ("Loaded {0} records out of {1} ..", ins, torrs.Count);

            }

            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var ins = db.UpdateAll (torrs);

                Console.WriteLine ("Updated {0} records out of {1} ..", ins, torrs.Count);

                return ins;
            }

        }

        public String GetNextHashId ()
        {
            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var query = "SELECT * FROM MTorr WHERE (Processed <> true) ORDER BY CountSeen DESC LIMIT 1";
                var mTorr = db.Query<MTorr> (query).FirstOrDefault ();

                if (ac.DEBUG_MODE)
                    Console.WriteLine ("GetNextHashId()  Found Torrent {0}, countSeen {2}, processedTime {1}",
                        mTorr.HashId,
                        mTorr.ProcessedTime,
                        mTorr.CountSeen
                        );

                mTorr.Processed = true;
                mTorr.ProcessedTime = DateTime.UtcNow;

                var upds = db.Update (mTorr);

                if (ac.DEBUG_MODE)
                    Console.WriteLine ("GetNextHashId()  Found Torrent {0}, updated {1} record", mTorr.HashId, upds);

                return mTorr.HashId;
            }
        }

        public bool HasBeenDownloaded (MDownloadedFile mDownloadedFile)
        {
            using (var db = new SQLiteConnection (ac.SDB_DLD_URL, SQLiteOpenFlags.ReadOnly)) {
                return db.ExecuteScalar<int> (
                    "SELECT COUNT(*) FROM MDownloadedFile M WHERE (M.FileName = ? AND M.LENGTH = ?)",
                    mDownloadedFile.FileName,
                    mDownloadedFile.Length) > 0;
            }
        }

        public void UpdateHashId (MTorr mTorrentU)
        {
            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var mTorr = db.Query<MTorr> ("SELECT * FROM MTorr WHERE HashId = ? LIMIT 1", mTorrentU.HashId).FirstOrDefault ();

                if (ac.DEBUG_MODE)
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

                if (ac.DEBUG_MODE)
                    Console.WriteLine ("UpdateHashId()   Found Torrent {0}, updated {1} record", mTorr.HashId, upds);
            }
        }

        public void LoadHashesFromFile (String inputFile) //TODO: this should be split in IO and DAO
        {
            var lines = File.ReadAllLines (inputFile);

            Console.WriteLine ("Loaded {0} lines from file [{1}]", lines.Length, inputFile);
            //Console.ReadLine ();

            var mTorrs = new List<MTorrLog> ();

            foreach (var line in lines) {
                if (line.Length < 50) { continue; }

                try {
                    var segs = line.Split (' ');
                    var dateSeen = DateTime.Parse (segs[1]);
                    var hashId = segs[4].Substring (1, 40).ToLower ();

                    mTorrs.Add (new MTorrLog () {
                        HashId = hashId,
                        SeenAt = dateSeen
                    });

                } catch (Exception ex) {
                    Console.Error.WriteLine ("Error processing line [{0}] {1}", line, ex.Message);
                }

                // Console.WriteLine ("Date [{0}], HashId [{1}]", dateSeen, hashId);
            }

            // SELECT DISTINCT HashId, count(hashid) as m from MTorrLog group by hashid order by m desc

            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var ins = db.InsertAll (mTorrs, " OR IGNORE ");

                Console.WriteLine ("Loaded {0} records to Log Table out of {1} ..", ins, lines.Length);
            }

            // insert new records
            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var ins = db.Execute (@"INSERT INTO MTorr (HashId, CountSeen, LastSeen, Processed)
                                        SELECT DISTINCT 
                                          HashId, COUNT(HashId) AS CountSeen, MAX(SeenAt) AS LastSeen, 0 as Processed
                                        FROM
                                          MTorrLog
                                        WHERE
                                          HashId NOT IN (SELECT DISTINCT HashId FROM MTorr)
                                        GROUP BY HashId
                                        ORDER BY CountSeen DESC");

                Console.WriteLine ("Loaded {0} records to Tor Table out of {1} ..", ins, lines.Length);
            }

            // update counts and lastseen
            using (var db = new SQLiteConnection (ac.SDB_URL)) {
                var ins = db.Execute (@"UPDATE MTorr
                                        SET
                                            CountSeen = (SELECT COUNT(MTorrLog.HashId) AS CountSeen FROM MTorrLog WHERE MTorr.HashId = MTorrLog.HashId GROUP BY MTorrLog.HashId),
                                            LastSeen  = (SELECT   MAX(MTorrLog.SeenAt) AS LastSeen  FROM MTorrLog WHERE MTorr.HashId = MTorrLog.HashId GROUP BY MTorrLog.HashId)
                                        WHERE EXISTS (
                                            SELECT HashId FROM MTorrLog WHERE MTorrLog.HashId = MTorr.HashId
                                        )");

                Console.WriteLine ("Updated stats of {0} records to Tor Table ..", ins);
            }

        }
    }
}
