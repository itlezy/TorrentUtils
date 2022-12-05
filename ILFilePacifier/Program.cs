using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using Alphaleonis.Win32.Filesystem;

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

    -----------------------------------------

    Lines can be commented with #

     * */

    class Program
    {

        static readonly string FILE_WHERES = ConfigurationManager.AppSettings["FILE_WHERES"];
        static readonly string FILE_MATCHERS = ConfigurationManager.AppSettings["FILE_MATCHERS"];
        static readonly int FILE_MIN_SIZE_MB = int.Parse (ConfigurationManager.AppSettings["FILE_MIN_SIZE_MB"]);

        static void Main (string[] args)
        {

            var wheres = new List<String> ();

            foreach (var where in File.ReadAllLines (FILE_WHERES)) {
                if (!String.IsNullOrWhiteSpace (where) && where.IndexOf ('#') != 0 && Directory.Exists (where)) {
                    wheres.Add (where);
                }
            }

            var matchers = ILMatch.LoadFromCSV (FILE_MATCHERS);

            foreach (var matcher in matchers) {

                var foundsdd = new List<String> ();
                var foundsff = new List<String> ();

                foreach (var where in wheres) {

                    foreach (var what in matcher.What) {

                        // find matching dirs
                        try {
                            var dd = Directory.GetDirectories (
                                where,
                                what.ToWildcard (),
                                System.IO.SearchOption.AllDirectories
                            );

                            foreach (var d in dd) {
                                var di = new DirectoryInfo (d);

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
                                what.ToWildcard (),
                                System.IO.SearchOption.AllDirectories
                            );

                            foreach (var f in ff) {
                                var fi = new FileInfo (f);

                                if (fi.Length > (FILE_MIN_SIZE_MB * 1024 * 1024) && !foundsff.Contains (f))
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
            foreach (var foundDirFullName in foundsdd) {
                var targetDirFullName = Path.GetDirectoryName (matcher.Destination) + Path.DirectorySeparator + new DirectoryInfo (foundDirFullName).Name;

                var lines = new String[] {
                        "REM >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\r\n\r\n",

                        string.Format (
                            "REM C {0} / {1}\r\n" +
                            "REM W [ {2} ]\r\n" +
                            "REM F \"{3}\"\r\n" +
                            "REM T \"{4}\"\r\n\r\n",
                            ++c, foundsdd.Count, matcher.What.FirstOrDefault (), foundDirFullName, targetDirFullName),

                        string.Format (
                            "TITLE PROCESSING D {0} / {1} [ {2} ] \"{3}\" TO \"{4}\"",
                            c, foundsdd.Count, matcher.What.FirstOrDefault (), foundDirFullName, targetDirFullName
                        ).Truncate(240),

                        "\r\n\r\n",

                        string.Format (
                            "IF EXIST             \"{0}\" (\r\n" +
                            "  ROBOCOPY           \"{0}\" \"{1}\" /S /J /MOV\r\n" +
                            "  ATTRIB   -R -S -H  \"{0}\"\r\n" +
                            "  RMDIR              \"{0}\"\r\n" +
                            ")\r\n\r\n",

                            foundDirFullName, //0
                            targetDirFullName //1
                        ),

                        "REM <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\r\n\r\n\r\n"

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
            foreach (var foundFileFullName in foundsff) {
                var targetFileFullName = Path.GetDirectoryName (matcher.Destination) + Path.DirectorySeparator + Path.GetFileName (foundFileFullName);
                var targetDirFullName = Path.GetDirectoryName (matcher.Destination);

                var lines = new String[] {
                        "REM >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\r\n\r\n",

                        string.Format (
                            "REM C {0} / {1}\r\n" +
                            "REM W [ {2} ]\r\n" +
                            "REM F \"{3}\"\r\n" +
                            "REM T \"{4}\"\r\n\r\n",
                            ++c, foundsff.Count, matcher.What.FirstOrDefault (), foundFileFullName, targetDirFullName),

                        string.Format (
                            "TITLE PROCESSING F {0} / {1} [ {2} ] \"{3}\" TO \"{4}\"",
                            c, foundsff.Count, matcher.What.FirstOrDefault (), foundFileFullName, targetDirFullName
                        ).Truncate(240),

                        "\r\n\r\n",

                        string.Format (
                            "IF EXIST             \"{0}\" (\r\n" +
                            "IF EXIST             \"{1}\" (\r\n" +
                            "  Certutil -hashfile \"{1}\"\r\n" +
                            "  Certutil -hashfile \"{0}\"\r\n" +
                            "  PAUSE\r\n" +
                            " )\r\n" +
                            ")\r\n\r\n",

                            targetFileFullName, //0
                            foundFileFullName //1
                        ),

                        string.Format (
                            "IF EXIST             \"{3}\" (\r\n" +
                            "  ATTRIB   -R -S -H  \"{3}\"\r\n" +
                            "  ROBOCOPY           \"{0}\" \"{1}\" \"{2}\" /J /MOV\r\n" +
                            ")\r\n\r\n",
                            Path.GetDirectoryName(foundFileFullName), //0
                            targetDirFullName, //1
                            Path.GetFileName(foundFileFullName), //2
                            foundFileFullName, //3
                            targetFileFullName //4
                        ),

                        "REM <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\r\n\r\n\r\n"

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
