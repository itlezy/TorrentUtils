﻿using System.Configuration;

namespace ILCommon.Config
{
    public static class Constants
    {
        public const string REGEX_SHA = "\\b[0-9a-f]{40}\\b";
    }

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

        /// <summary>
        /// File containing ban words to avoid when downloading metadata, one word per line
        /// </summary>
        public string BAN_WORDS_FILE = ConfigurationManager.AppSettings["BAN_WORDS_FILE"];

        public readonly bool DEBUG_MODE = bool.Parse (ConfigurationManager.AppSettings["DEBUG_MODE"]);

        public readonly string TORR_EXT_WILDCARD = ConfigurationManager.AppSettings["TORR_EXT_WILDCARD"];

    }
}
