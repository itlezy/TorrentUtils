using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTorrents
{
    class ATConfig
    {
        public readonly String TORR_EXT_WILDCARD = ConfigurationManager.AppSettings["TORR_EXT_WILDCARD"];
        public readonly String TORR_ARCHIVE_DIR = ConfigurationManager.AppSettings["TORR_ARCHIVE_DIR"];
        public readonly String TORR_ARCHIVE_DIR_OLD = ConfigurationManager.AppSettings["TORR_ARCHIVE_DIR_OLD"];
        public readonly String TORR_ARCHIVE_REG = ConfigurationManager.AppSettings["TORR_ARCHIVE_REG"];
        public readonly String TORR_ARCHIVE_FILES_REG = ConfigurationManager.AppSettings["TORR_ARCHIVE_FILES_REG"];
        public readonly String TORR_INCOMING_DIR = ConfigurationManager.AppSettings["TORR_INCOMING_DIR"];
        public readonly String TORR_INPUT_DIR = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile) + @"\Downloads";

        public readonly string SDB_DLD_URL = ConfigurationManager.AppSettings["SDB_DLD_URL"];

    }
}
