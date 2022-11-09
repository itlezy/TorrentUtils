using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

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

                var founds = new List<String> ();

                foreach (var where in wheres) {

                    foreach (var what in matcher.What) {
                        try {
                            var ff = Directory.GetFiles (
                                where,
                                "*" + what.Trim ().Replace (' ', '*') + "*",
                                System.IO.SearchOption.AllDirectories
                                );

                            foreach (var f in ff) {
                                var fi = new FileInfo (f);

                                if (fi.Length > (1024 * 1024))
                                    founds.Add (f);

                            }
                        } catch (Exception x) {
                            Console.Error.WriteLine ("Err {0}", x.Message);
                        }
                    }

                }

                foreach (var found in founds) {
                    File.AppendAllLines (
                        "Z_M_" + matcher.What.FirstOrDefault () + ".cmd",
                        new String[] { string.Format ("ROBOCOPY \"{0}\" \"{1}\" \"{2}\" /MOV\r\n",
                        Path.GetDirectoryName(found),
                        Path.GetDirectoryName(matcher.Destination),
                        Path.GetFileName(found)
                        ) },
                        Encoding.UTF8
                        );
                }

            }

        }
    }
}
