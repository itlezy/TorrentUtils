﻿using System.Configuration;

namespace MetadataDownloader
{
    class MDConfig : ILCommon.Config.CommonConfig
    {
        // The timeout will be determined by TORRENT_PARALLEL_LIMIT * MAIN_LOOP_INTERVAL as torrents get removed on a FIFO logic basis
        public readonly int MAIN_LOOP_INTERVAL = int.Parse (ConfigurationManager.AppSettings["MAIN_LOOP_INTERVAL"]);
        public readonly int TORRENT_PARALLEL_LIMIT = int.Parse (ConfigurationManager.AppSettings["TORRENT_PARALLEL_LIMIT"]);
        public readonly int TORRENT_STOP_TIMEOUT = int.Parse (ConfigurationManager.AppSettings["TORRENT_STOP_TIMEOUT"]);
        public readonly string TORRENT_OUTPUT_PATH = ConfigurationManager.AppSettings["TORRENT_OUTPUT_PATH"];

        public readonly string TMP_SAVE_DIR = ConfigurationManager.AppSettings["TMP_SAVE_DIR"];
        public readonly string MAGNET_PREFIX = ConfigurationManager.AppSettings["MAGNET_PREFIX"];

        /// <summary>
        /// File containing ban words to avoid when downloading metadata, one word per line
        /// </summary>
        public string BAN_WORDS_FILE = ConfigurationManager.AppSettings["BAN_WORDS_FILE"];

    }
}
