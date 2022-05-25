using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataDownloader
{
    class MDConfig
    {
        // The timeout will be determined by TORRENT_PARALLEL_LIMIT * MAIN_LOOP_INTERVAL as torrents get removed on a FIFO logic basis
        public readonly int MAIN_LOOP_INTERVAL = int.Parse (ConfigurationManager.AppSettings["MAIN_LOOP_INTERVAL"]);
        public readonly int TORRENT_PARALLEL_LIMIT = int.Parse (ConfigurationManager.AppSettings["TORRENT_PARALLEL_LIMIT"]);
        public readonly int TORRENT_STOP_TIMEOUT = int.Parse (ConfigurationManager.AppSettings["TORRENT_STOP_TIMEOUT"]);
        public readonly string TORRENT_OUTPUT_PATH = ConfigurationManager.AppSettings["TORRENT_OUTPUT_PATH"];
        public readonly string DB_URL = ConfigurationManager.AppSettings["DB_URL"];
        public readonly string DB_NAME = ConfigurationManager.AppSettings["DB_NAME"];
        public readonly string DB_COLLECTION_NAME = ConfigurationManager.AppSettings["DB_COLLECTION_NAME"];
        public readonly string TMP_SAVE_DIR = ConfigurationManager.AppSettings["TMP_SAVE_DIR"];
        public readonly string MAGNET_PREFIX = ConfigurationManager.AppSettings["MAGNET_PREFIX"];

        public readonly string SDB_URL = ConfigurationManager.AppSettings["SDB_URL"];
        public readonly string SDB_DLD_URL = ConfigurationManager.AppSettings["SDB_DLD_URL"];

        public readonly bool DEBUG_MODE = false;
    }
}
