
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
        public string InputLogFile { get; set; }

        [Option ('t', "loadtorrents", Required = false, HelpText = "Load downloaded torrents to DB")]
        public bool LoadDownloadedTorrents { get; set; }

        [Option ('r', "printstats", Required = false, HelpText = "Print DB Stats")]
        public bool PrintStats { get; set; }

        [Option ('d', "inputdir", Required = false, HelpText = "Input directory to search for files")]
        public string InputDir { get; set; }

        [Option ('x', "runtests", Required = false, HelpText = "Run a set of tests")]
        public bool RunTests { get; set; }

        [Option ('b', "cleanbanwords", Required = false, HelpText = "Clean ban words from DB")]
        public bool CleanBanWords { get; set; }
    }
}
