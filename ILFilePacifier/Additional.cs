using System;
using System.Collections.Generic;
using System.Text;

using Alphaleonis.Win32.Filesystem;

namespace ILFilePacifier
{
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

                if (!String.IsNullOrWhiteSpace (line) && line.IndexOf ('#') != 0 && line.IndexOf (";") > 0) {
                    var match = new ILMatch ();

                    // sample line : Bob Marley, Bob Marly;o:\music\Bob Marley
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

    public static class StringExt
    {
        public static string Truncate (this string value, int maxLength)
        {
            if (string.IsNullOrEmpty (value))
                return value;

            return value.Length <= maxLength ? value : value.Substring (0, maxLength);
        }

        public static string ToWildcard (this string value)
        {
            if (string.IsNullOrEmpty (value))
                return value;

            return "*" + value.Trim ().Replace (' ', '*') + "*";
        }

    }
}
