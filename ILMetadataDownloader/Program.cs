using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

using ILCommon;
using CommandLine;


namespace MetadataDownloader
{
    partial class Program
    {
        static LockManager lockManager = new LockManager ();

        static void Main (string[] args)
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
            } else if (opts.LoadHashes && File.Exists (opts.InputLogFile)) {

                new DAO ().LoadHashesFromFile (opts.InputLogFile);
            } else if (opts.LoadDownloadedTorrents && Directory.Exists (opts.InputDir)) {

                new QueueManager ().LoadDownloadedTorrents (opts.InputDir);
            } else if (opts.RunTests) {

                new TestManager ().RunTests ();
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
