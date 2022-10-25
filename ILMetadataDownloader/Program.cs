using System;
using System.Collections.Generic;
using System.Threading;

using Alphaleonis.Win32.Filesystem;

using CommandLine;

using ILCommon;
using ILCommon.IO;

using MetadataDownloader.Data;

namespace MetadataDownloader
{
    partial class Program
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
            } else if (opts.LoadHashes) {

                if (!String.IsNullOrWhiteSpace (opts.InputLogFile))
                    new DAO ().LoadHashesFrom (opts.InputLogFile);
                else
                    new DAO ().LoadHashesFrom (opts.InputDir);

            } else if (opts.LoadDownloadedTorrents) {

                new QueueManager ().LoadDownloadedTorrents (opts.InputDir);
            } else if (opts.PrintStats) {

                new DAO ().PrintTableStats ();
            } else if (opts.RunTests) {

                new TestManager ().RunTests ();
            } else if (opts.CleanBanWords) {

                new DAO ().CleanBanWords ();

                if (!String.IsNullOrWhiteSpace (opts.InputDir) && Directory.Exists (opts.InputDir)) {
                    new IOManager ().CleanBanWords (opts.InputDir);
                }
            } else {

                MetadataDownload ();
            }
        }

        static void HandleParseError (IEnumerable<Error> errs)
        {
            //handle errors
            Console.Error.WriteLine ("Command line nope!");
        }

        static void MetadataDownload ()
        {
            new IOManager ().CleanDirs (new string[] { new MDConfig ().TMP_SAVE_DIR, "cache" });

            CancellationTokenSource cancellation = new CancellationTokenSource ();

            var task = new QueueManager ().MainLoop (cancellation.Token);

            // We need to cleanup correctly when the user closes the window by using ctrl-c
            // or an unhandled exception happens

            Console.CancelKeyPress += delegate {
                lockManager.ReleaseLock ();

                cancellation.Cancel ();
                task.Wait ();
            };

            AppDomain.CurrentDomain.ProcessExit += delegate {
                lockManager.ReleaseLock ();

                cancellation.Cancel ();
                task.Wait ();
            };

            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) {
                lockManager.ReleaseLock ();

                Console.WriteLine (e.ExceptionObject);
                cancellation.Cancel ();
                task.Wait ();
            };

            Thread.GetDomain ().UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) {
                lockManager.ReleaseLock ();

                Console.WriteLine (e.ExceptionObject);
                cancellation.Cancel ();
                task.Wait ();
            };

            task.Wait ();
        }
    }
}
