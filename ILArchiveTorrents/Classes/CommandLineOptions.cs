using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;

namespace ArchiveTorrents
{
    class CommandLineOptions
    {
        [Option ('c', "createtables", Required = false, HelpText = "Initialize DB tables")]
        public bool CreateTables { get; set; }

        [Option ('f', "loadfiles", Required = false, HelpText = "Load downloaded files (name & length) to DB")]
        public bool LoadDownloadedFiles { get; set; }

        [Option ('t', "loadtorrents", Required = false, HelpText = "Load downloaded torrents to DB")]
        public bool LoadDownloadedTorrents { get; set; }

        [Option ('d', "inputdir", Required = false, HelpText = "Input directory to search for files")]
        public String InputDir { get; set; }


    }
}
