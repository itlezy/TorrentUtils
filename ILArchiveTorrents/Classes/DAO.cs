using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILCommon.Model;

using SQLite;

namespace ArchiveTorrents
{
    class DAO
    {
        ATConfig ac = new ATConfig ();

        public void CreateTables ()
        {
            using (var db = new SQLiteConnection (ac.SDB_DLD_URL)) {
                db.CreateTable<MDownloadedTorr> ();
                db.CreateTable<MDownloadedFile> ();
            }

            using (var db = new SQLiteConnection (ac.SDB_DLD_URL)) {
                var ins = db.Execute (
                    "CREATE UNIQUE INDEX \"MTorrFileSummary_UQ\" ON \"MDownloadedTorr\" ( \"FileName\"    ASC, \"Length\"    ASC )"
                );

            }

        }
        public int LoadDownloadedFiles (List<MDownloadedFile> files)
        {
            using (var db = new SQLiteConnection (ac.SDB_DLD_URL)) {
                var ins = db.InsertAll (files, " OR IGNORE ");

                return ins;
            }

        }

        public int LoadDownloadedTorrents (List<MDownloadedTorr> torrs)
        {
            using (var db = new SQLiteConnection (ac.SDB_DLD_URL)) {
                var ins = db.InsertAll (torrs, " OR IGNORE ");

                return ins;
            }
        }

        public bool HasBeenDownloaded (String hashId)
        {
            using (var db = new SQLiteConnection (ac.SDB_DLD_URL, SQLiteOpenFlags.ReadOnly)) {
                return db.ExecuteScalar<int> (
                    "SELECT COUNT(*) FROM MDownloadedTorr M WHERE (M.HashId = ?)",
                    hashId) > 0;
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
    }
}
