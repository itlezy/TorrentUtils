
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

        [Option ('r', "cachetorrents", Required = false, HelpText = "Cache torrents to torrage")]
        public bool CacheDownloadedTorrents { get; set; }

        [Option ('k', "skipcopy", Required = false, HelpText = "Skip Copying torrents to Incoming Dir of BT Client")]
        public bool SkipCopyTorrents { get; set; }

        [Option ('s', "synctorrents", Required = false, HelpText = "Sync Torrents from BT Client to Archive Dir")]
        public bool SyncDownloadedTorrents { get; set; }

        [Option ('d', "inputdir", Required = false, HelpText = "Input directory to search for files")]
        public string InputDir { get; set; }

        [Option ('x', "extension", Required = false, HelpText = "Process this extension from the Input Dir")]
        public string FileExtension { get; set; }

        [Option ('n', "nullfiles", Required = false, HelpText = "Delete null files from the Input Dir")]
        public bool NullFiles { get; set; }
    }
}
