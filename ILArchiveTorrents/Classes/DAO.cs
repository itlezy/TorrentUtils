using System.Collections.Generic;

using ILCommon.Data;
using ILCommon.Data.Model;

using SQLite;

namespace ArchiveTorrents
{
    class DAO : BaseDAO
    {
        readonly ATConfig c = new ATConfig ();

        public void CreateTables ()
        {
            using (var db = new SQLiteConnection (c.SDB_DLD_URL)) {
                db.CreateTable<MDownloadedTorr> ();
                db.CreateTable<MDownloadedFile> ();
            }

            using (var db = new SQLiteConnection (c.SDB_DLD_URL)) {
                var ins = db.Execute (
                    "CREATE UNIQUE INDEX \"MTorrFileSummary_UQ\" ON \"MDownloadedTorr\" ( \"FileName\"    ASC, \"Length\"    ASC )"
                );

            }

        }
        public int LoadDownloadedFiles (List<MDownloadedFile> files)
        {
            using (var db = new SQLiteConnection (c.SDB_DLD_URL)) {
                var ins = db.InsertAll (files, " OR IGNORE ");

                return ins;
            }

        }

        public int LoadDownloadedTorrents (List<MDownloadedTorr> torrs)
        {
            using (var db = new SQLiteConnection (c.SDB_DLD_URL)) {
                var ins = db.InsertAll (torrs, " OR IGNORE ");

                return ins;
            }
        }

        public bool HasBeenDownloaded (string hashId)
        {
            using (var db = new SQLiteConnection (c.SDB_DLD_URL, SQLiteOpenFlags.ReadOnly)) {
                return db.ExecuteScalar<int> (
                    "SELECT COUNT(*) FROM MDownloadedTorr M WHERE (M.HashId = ?)",
                    hashId) > 0;
            }
        }
    }
}
