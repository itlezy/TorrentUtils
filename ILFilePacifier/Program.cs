﻿using System;
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

    matchers.csv

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

    public static class StringExt
    {
        public static string Truncate (this string value, int maxLength)
        {
            if (string.IsNullOrEmpty (value))
                return value;
            return value.Length <= maxLength ? value : value.Substring (0, maxLength);
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

            var matchers = ILMatch.LoadFromCSV (@"matchers.csv");

            foreach (var matcher in matchers) {

                var foundsdd = new List<String> ();
                var foundsff = new List<String> ();

                foreach (var where in wheres) {

                    foreach (var what in matcher.What) {

                        // find matching dirs
                        try {
                            var dd = Directory.GetDirectories (
                            where,
                            ILMatch.ToWildcard (what),
                            System.IO.SearchOption.AllDirectories
                            );

                            foreach (var d in dd) {
                                var di = new DirectoryInfo (d);

                                // && Operators.LikeString (d, ILMatch.ToWildcard(what), Microsoft.VisualBasic.CompareMethod.Text))

                                if (di.GetFiles ().Length > 0 && !foundsdd.Contains (d))
                                    foundsdd.Add (d);

                            }
                        } catch (Exception x) {
                            Console.Error.WriteLine ("Err {0}", x.Message);
                        }

                        // find matching files
                        try {
                            var ff = Directory.GetFiles (
                                where,
                                ILMatch.ToWildcard (what),
                                System.IO.SearchOption.AllDirectories
                                );

                            foreach (var f in ff) {
                                var fi = new FileInfo (f);

                                if (fi.Length > (90 * 1024 * 1024) && !foundsff.Contains (f))
                                    foundsff.Add (f);

                            }
                        } catch (Exception x) {
                            Console.Error.WriteLine ("Err {0}", x.Message);
                        }
                    }

                }

                GenerateMoveCmds_Dir (matcher, foundsdd);
                GenerateMoveCmds_File (matcher, foundsff);

            }

        }

        private static void GenerateMoveCmds_Dir (ILMatch matcher, List<string> foundsdd)
        {
            int c = 0;
            foreach (var found in foundsdd) {
                string targetDir = Path.GetDirectoryName (matcher.Destination) + Path.DirectorySeparator + new DirectoryInfo (found).Name;

                var lines = new String[] {
                        string.Format (
                            "TITLE PROCESSING D {0} / {1} [ {2} ] \"{3}\" TO \"{4}\"",
                            ++c, foundsdd.Count, matcher.What.FirstOrDefault (), found, targetDir
                        ).Truncate(240),

                        "\r\n\r\n",

                        //// in case the target directory exists, compare the content files

                        //string.Format ("IF EXIST   \"{0}\" (\r\n" +
                        //"  REM Certutil -hashfile \"{1}\"\r\n" +
                        //"  REM Certutil -hashfile \"{2}\"\r\n" +
                        //"  REM PAUSE\r\n" +
                        //")\r\n\r\n",

                        //targetDir,

                        //Directory.GetFiles(found).FirstOrDefault(),
                        //Directory.Exists(targetDir) && Directory.GetFiles(targetDir).Length > 0 ? Directory.GetFiles(targetDir).FirstOrDefault() : "NOFILE"

                        //),

                        string.Format (
                            "IF EXIST             \"{0}\" (\r\n" +
                            "  ROBOCOPY           \"{0}\" \"{1}\" /MOV\r\n" +
                            "  ATTRIB -R -S -H    \"{0}\"\r\n" +
                            "  RMDIR              \"{0}\"\r\n" +
                            ")\r\n\r\n",

                            found,
                            targetDir
                        ),

                        "REM ----------------------------------------------------------------------------------------------------------\r\n\r\n\r\n"

                    };

                File.AppendAllLines (
                    "Z_!ALL_1D.cmd",
                    lines,
                    Encoding.Default);

                File.AppendAllLines (
                    "Z_!ALL_0ALL.cmd",
                    lines,
                    Encoding.Default);

                File.AppendAllLines (
                    "Z_" + matcher.What.FirstOrDefault () + "_1D.cmd",
                    lines,
                    Encoding.Default);
            }
        }

        private static void GenerateMoveCmds_File (ILMatch matcher, List<string> foundsff)
        {
            int c = 0;
            foreach (var found in foundsff) {
                var lines = new String[] {
                        string.Format (
                            "TITLE PROCESSING F {0} / {1} [ {2} ] \"{3}\" TO \"{4}\"",
                            ++c, foundsff.Count, matcher.What.FirstOrDefault (), found, Path.GetDirectoryName(matcher.Destination)
                        ).Truncate(240),

                        "\r\n\r\n",

                        string.Format (
                            "IF EXIST             \"{0}\" (\r\n" +
                            "  Certutil -hashfile \"{1}\"\r\n" +
                            "  Certutil -hashfile \"{0}\"\r\n" +
                            "  PAUSE\r\n" +
                            ")\r\n\r\n",

                            Path.GetDirectoryName(matcher.Destination) + Path.DirectorySeparator + Path.GetFileName(found),
                            found
                        ),

                        string.Format (
                            "IF EXIST             \"{3}\" (\r\n" +
                            "  ATTRIB -R -S -H    \"{3}\"\r\n" +
                            "  ROBOCOPY           \"{0}\" \"{1}\" \"{2}\" /MOV\r\n" +
                            ")\r\n\r\n",
                            Path.GetDirectoryName(found),
                            Path.GetDirectoryName(matcher.Destination),
                            Path.GetFileName(found),
                            found
                        ),

                        "REM ----------------------------------------------------------------------------------------------------------\r\n\r\n\r\n"

                    };

                File.AppendAllLines (
                    "Z_!ALL_2F.cmd",
                    lines,
                    Encoding.Default);

                File.AppendAllLines (
                    "Z_!ALL_0ALL.cmd",
                    lines,
                    Encoding.Default);

                File.AppendAllLines (
                    "Z_" + matcher.What.FirstOrDefault () + "_2F.cmd",
                    lines,
                    Encoding.Default);
            }
        }
    }
}
