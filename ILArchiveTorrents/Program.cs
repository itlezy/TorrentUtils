using System;
using System.Collections.Generic;

using Alphaleonis.Win32.Filesystem;

using CommandLine;

using ILCommon;

namespace ArchiveTorrents
{
    class Program
    {
        static readonly LockManager lockManager = new LockManager ();

        static void Main (string[] args)
        {
            try {
                if (lockManager.AcquireLock ()) {

                    CommandLine.Parser.Default.ParseArguments<CommandLineOptions> (args)
                        .WithParsed (RunOptions)
                        .WithNotParsed (HandleParseError);

                } else {
                    Console.Error.WriteLine ("Application running, please check .lck file. Exiting..");
                }

            } finally {
                lockManager.ReleaseLock ();
            }


        }

        static void RunOptions (CommandLineOptions opts)
        {
            if (opts.CreateTables) {

                new DAO ().CreateTables ();
            } else if (opts.CacheDownloadedTorrents) {

                if (Directory.Exists (opts.InputDir))
                    new ArchiveManager ().CacheDownloadedTorrents (opts.InputDir);
                else
                    Console.Error.WriteLine ("Directory not found '{0}'", opts.InputDir);

            } else if (opts.LoadDownloadedTorrents) {

                if (Directory.Exists (opts.InputDir))
                    new ArchiveManager ().LoadDownloadedTorrents (opts.InputDir, opts.FileExtension);
                else
                    Console.Error.WriteLine ("Directory not found '{0}'", opts.InputDir);

            } else if (opts.LoadDownloadedFiles) {

                if (Directory.Exists (opts.InputDir))
                    new ArchiveManager ().LoadDownloadedFiles (opts.InputDir);
                else
                    Console.Error.WriteLine ("Directory not found '{0}'", opts.InputDir);

            } else if (opts.NullFiles) {

                if (Directory.Exists (opts.InputDir))
                    new ArchiveManager ().DeleteNullFiles (opts.InputDir);
                else
                    Console.Error.WriteLine ("Directory not found '{0}'", opts.InputDir);

            } else if (opts.SyncDownloadedTorrents) {

                if (Directory.Exists (opts.InputDir))
                    new ArchiveManager ().SyncDownloadedTorrents (opts.InputDir, opts.FileExtension);
                else
                    Console.Error.WriteLine ("Directory not found '{0}'", opts.InputDir);

            } else if (opts.SkipCopyTorrents) {

                new ArchiveManager ().RemDupsAndArchive (true);
            } else {

                new ArchiveManager ().RemDupsAndArchive (false);
            }
        }

        static void HandleParseError (IEnumerable<Error> errs)
        {
            //handle errors
            Console.Error.WriteLine ("Command line nope!");
        }
    }
}
