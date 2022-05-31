
using ILCommon.Data.Model;

using SQLite;

namespace ILCommon.Data
{
    public class BaseDAO
    {
        readonly Config.CommonConfig c = new Config.CommonConfig ();

        public bool HasBeenDownloaded (MDownloadedFile mDownloadedFile)
        {
            using (var db = new SQLiteConnection (c.SDB_DLD_URL, SQLiteOpenFlags.ReadOnly)) {
                return db.ExecuteScalar<int> (
                    "SELECT COUNT(*) FROM MDownloadedFile M WHERE (M.FileName = ? AND M.LENGTH = ?)",
                    mDownloadedFile.FileName,
                    mDownloadedFile.Length) > 0;
            }
        }
    }
}
