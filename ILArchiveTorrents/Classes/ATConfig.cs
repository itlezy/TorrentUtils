using System;
using System.Configuration;

namespace ArchiveTorrents
{
    class ATConfig : ILCommon.Config.CommonConfig
    {
        public readonly string TORR_HASH_EXT_WILDCARD = ConfigurationManager.AppSettings["TORR_HASH_EXT_WILDCARD"];
        public readonly string TORR_ARCHIVE_DIR = ConfigurationManager.AppSettings["TORR_ARCHIVE_DIR"];
        public readonly string TORR_ARCHIVE_DIR_CONFIG = ConfigurationManager.AppSettings["TORR_ARCHIVE_DIR_CONFIG"];
        public readonly string TORR_ARCHIVE_DIR_OLD = ConfigurationManager.AppSettings["TORR_ARCHIVE_DIR_OLD"];
        public readonly string TORR_ARCHIVE_REG = ConfigurationManager.AppSettings["TORR_ARCHIVE_REG"];
        public readonly string TORR_ARCHIVE_FILES_REG = ConfigurationManager.AppSettings["TORR_ARCHIVE_FILES_REG"];
        public readonly string TORR_INCOMING_DIR = ConfigurationManager.AppSettings["TORR_INCOMING_DIR"];

        public readonly string TORR_INPUT_DIR =
            Alphaleonis.Win32.Filesystem.Directory.Exists (ConfigurationManager.AppSettings["TORR_INPUT_DIR"]) ?
            ConfigurationManager.AppSettings["TORR_INPUT_DIR"] :
            Environment.GetFolderPath (Environment.SpecialFolder.UserProfile) + @"\Downloads";

    }
}
