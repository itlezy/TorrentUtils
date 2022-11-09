using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

using Microsoft.VisualBasic.CompilerServices;

namespace ILFilePacifier
{
    /**

    Generates ROBOCOPY commands to move files from various sources to a common destination.
    i.e.

    matches.csv

    what to match;where to move
    -----------------------------------------
    Bob Marley, Bob Marly;o:\music\Bob Marley

    wheres.txt

    where to search
    -----------------------------------------
    g:\old_music
    h:\archive_music

     * */

    class ILMatch
    {
        public List<string> What { get; }
        public List<string> Where { get; }

        public string Destination { get; set; }

        public ILMatch ()
        {
            What = new List<string> ();
            Where = new List<string> ();
        }

        public static string ToWildcard (string inp)
        {
            return "*" + inp.Trim ().Replace (' ', '*') + "*";
        }

        public static List<ILMatch> LoadFromCSV (string filePath)
        {
            var lines = File.ReadAllLines (filePath, Encoding.UTF8);
            var matches = new List<ILMatch> ();

            foreach (var line in lines) {

                if (!String.IsNullOrWhiteSpace (line) && line.IndexOf (";") > 0) {
                    var match = new ILMatch ();

                    var whats = line.Split (';')[0];

                    foreach (var what in whats.Split (',')) {
                        match.What.Add (what.Trim ());
                    }

                    match.Destination = line.Split (';')[1];

                    matches.Add (match);
                }
            }


            return matches;
        }

    }

    class Program
    {
        static void Main (string[] args)
        {

            var wheres = new List<String> ();

            foreach (var where in File.ReadAllLines (@"wheres.txt")) {
                if (!String.IsNullOrWhiteSpace (where) && Directory.Exists (where)) {
                    wheres.Add (where);
                }
            }

            var matchers = ILMatch.LoadFromCSV (@"matches.csv");

            foreach (var matcher in matchers) {

                var foundsff = new List<String> ();
                var foundsdd = new List<String> ();

                foreach (var where in wheres) {

                    foreach (var what in matcher.What) {
                        try {
                            var ff = Directory.GetFiles (
                                where,
                                ILMatch.ToWildcard (what),
                                System.IO.SearchOption.AllDirectories
                                );

                            foreach (var f in ff) {
                                var fi = new FileInfo (f);

                                if (fi.Length > (1024 * 1024))
                                    foundsff.Add (f);

                            }
                        } catch (Exception x) {
                            Console.Error.WriteLine ("Err {0}", x.Message);
                        }

                        try {
                            var dd = Directory.GetDirectories (
                            where,
                            ILMatch.ToWildcard (what),
                            System.IO.SearchOption.AllDirectories
                            );

                            foreach (var d in dd) {
                                var di = new DirectoryInfo (d);

                                if (di.GetFiles ().Length > 0)// && Operators.LikeString (d, ILMatch.ToWildcard(what), Microsoft.VisualBasic.CompareMethod.Text))
                                    foundsdd.Add (d);
                            }
                        } catch (Exception x) {
                            Console.Error.WriteLine ("Err {0}", x.Message);
                        }
                    }

                }

                int c = 0;
                foreach (var found in foundsff) {
                    var lines = new String[] {
                        string.Format ("TITLE PROCESSING F {0} / {1} - \"{2}\"\r\n", ++c, foundsff.Count, found),

                        string.Format ("IF EXIST   \"{0}\" PAUSE \r\n",
                        Path.GetDirectoryName(matcher.Destination) + Path.DirectorySeparator + Path.GetFileName(found)
                        ),

                        string.Format ("IF EXIST   \"{3}\" (\r\n  ROBOCOPY \"{0}\" \"{1}\" \"{2}\" /MOV\r\n)\r\n\r\n",
                        Path.GetDirectoryName(found),
                        Path.GetDirectoryName(matcher.Destination),
                        Path.GetFileName(found),
                        found
                        ),

                    };

                    File.AppendAllLines (
                        "Z_" + matcher.What.FirstOrDefault () + "_2F.cmd",
                        lines,
                        Encoding.Default);
                }

                c = 0;
                foreach (var found in foundsdd) {
                    var lines = new String[] {
                        string.Format ("TITLE PROCESSING D {0} / {1} - \"{2}\"\r\n", ++c, foundsdd.Count, found),

                        string.Format ("IF EXIST   \"{0}\" PAUSE \r\n",
                        Path.GetDirectoryName(matcher.Destination) + Path.DirectorySeparator + new DirectoryInfo(found).Name
                        ),

                        string.Format ("IF EXIST   \"{0}\" (\r\n  ROBOCOPY \"{0}\" \"{1}\" /MOV\r\n)\r\n\r\n",
                        found,
                        Path.GetDirectoryName(matcher.Destination) + Path.DirectorySeparator + new DirectoryInfo(found).Name
                        ),

                    };

                    File.AppendAllLines (
                        "Z_" + matcher.What.FirstOrDefault () + "_1D.cmd",
                        lines,
                        Encoding.Default);
                }
            }

        }
    }
}
