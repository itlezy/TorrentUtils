using System.Configuration;

namespace ILCommon.Config
{
    public class CommonConfig
    {
        /// <summary>
        /// SQLite DB path of captured hash log and downloaded hashes
        /// </summary>
        public readonly string SDB_URL = ConfigurationManager.AppSettings["SDB_URL"];

        /// <summary>
        /// SQLite DB path of downloaded torrents
        /// </summary>
        public readonly string SDB_DLD_URL = ConfigurationManager.AppSettings["SDB_DLD_URL"];

        public readonly bool DEBUG_MODE = bool.Parse (ConfigurationManager.AppSettings["DEBUG_MODE"]);

    }
}
