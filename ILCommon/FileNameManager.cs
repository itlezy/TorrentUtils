using System;
using System.Collections.Generic;

using Alphaleonis.Win32.Filesystem;

namespace ILCommon
{
    public class FileNameManager
    {
        public bool ContainsBanWord (string utName, string utComment, string fName, IEnumerable<string> banWords)
        {
            foreach (var banWord in banWords) {
                if (
                    fName.IndexOf (banWord, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    utName.IndexOf (banWord, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    utComment.IndexOf (banWord, StringComparison.InvariantCultureIgnoreCase) >= 0) {
                    return true;
                }
            }

            return false;
        }

        public string NormalizeFileName (string originalName)
        {
            return originalName
                .Replace ("-[rarbg.to]", "")
                .Replace ("-[rarbg]", "")
                .Replace ("-[rbg.to]", "")
                .Replace ("-[rbg]", "")
                .Replace ("-[rartv]", "")

                .Replace ("[rarbg.to]", "")
                .Replace ("[rarbg]", "")
                .Replace ("[rbg.to]", "")
                .Replace ("[rartv]", "")
                .Replace ("[rbg]", "");
        }

        public bool IsMostlyLatin (string fileName)
        {
            var cc = fileName.ToCharArray ();
            var latinc = 0;

            foreach (char c in cc) {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || '.' == c || (c >= '0' && c <= '9')) {
                    latinc++;
                }
            }

            return ((double) latinc / (double) cc.Length) > 0.6;
        }

        public string GetSubCat (string fileName)
        {
            var subCat = "generic";

            if (fileName.Contains ("480") || fileName.Contains ("SD")) {
                subCat = "D0480p";
            } else if (fileName.Contains ("720")) {
                subCat = "D0720p";
            } else if (fileName.Contains ("1080") || fileName.ToLower ().Contains ("fhd")) {
                subCat = "D1080p";
            } else if (fileName.Contains ("2160") || fileName.ToLower ().Contains ("4k")) {
                subCat = "D2160p";
            }

            return subCat;
        }

        public string SafeName (string fileName, char replacementChar = '_')
        {
            var invalidChars = new HashSet<char> (Path.GetInvalidFileNameChars ());
            var fileNameCharArr = fileName.ToCharArray ();

            for (int i = 0; i < fileNameCharArr.Length; i++) {
                if (invalidChars.Contains (fileNameCharArr[i])) {
                    fileNameCharArr[i] = replacementChar;
                }
            }

            return new string (fileNameCharArr);
        }
    }
}
