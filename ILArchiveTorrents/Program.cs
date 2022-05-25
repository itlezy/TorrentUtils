using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using ILCommon;

using MonoTorrent;
using CommandLine;

using ILCommon.Model;

namespace ArchiveTorrents
{
    class Program
    {
        static LockManager lockManager = new LockManager ();

        static void Main (String[] args)
        {
            try {
                if (lockManager.AcquireLock ()) {

                    CommandLine.Parser.Default.ParseArguments<CommandLineOptions> (args)
                        .WithParsed (RunOptions)
                        .WithNotParsed (HandleParseError);

                }

            } finally {
                lockManager.ReleaseLock ();
            }


        }

        static void RunOptions (CommandLineOptions opts)
        {
            if (opts.CreateTables) {

                new DAO ().CreateTables ();
            } else if (opts.LoadDownloadedTorrents && Directory.Exists (opts.InputDir)) {

                new ArchiveManager ().LoadDownloadedTorrents (opts.InputDir);
            } else if (opts.LoadDownloadedFiles && Directory.Exists (opts.InputDir)) {

                new ArchiveManager ().LoadDownloadedFiles (opts.InputDir);
            } else {

                new ArchiveManager ().RemDupsAndArchive ();
            }
        }

        static void HandleParseError (IEnumerable<Error> errs)
        {
            //handle errors
            Console.Error.WriteLine ("Command line nope!");
        }


    }
}
