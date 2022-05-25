using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;

namespace MetadataDownloader
{
    class CommandLineOptions
    {
        [Option ('c', "createtables", Required = false, HelpText = "Initialize DB tables")]
        public bool CreateTables { get; set; }

        [Option ('s', "loadhashes", Required = false, HelpText = "Load hashes to DB")]
        public bool LoadHashes { get; set; }

        [Option ('i', "inputlogfile", Required = false, HelpText = "Input log files to load hashes to DB")]
        public String InputLogFile { get; set; }

        [Option ('t', "loadtorrents", Required = false, HelpText = "Load downloaded torrents to DB")]
        public bool LoadDownloadedTorrents { get; set; }

        [Option ('d', "inputdir", Required = false, HelpText = "Input directory to search for files")]
        public String InputDir { get; set; }

        [Option ('x', "runtests", Required = false, HelpText = "Run a set of tests")]
        public bool RunTests { get; set; }
    }
}
