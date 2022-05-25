using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILCommon
{
    public class FileNameManager
    {
        public String NormalizeFileName (String originalName)
        {
            return originalName
                .Replace ("-[rarbg.to]", "")
                .Replace ("[rarbg]", "");
        }

        public bool IsMostlyLatin (String fileName)
        {
            var cc = fileName.ToCharArray ();
            var latinc = 0;

            foreach (char c in cc) {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || '.' == c || (c >= '0' && c <= '9')) {
                    latinc++;
                }
            }

            return ((double) latinc / (double) cc.Length) > 0.5;
        }

        public String GetSubCat (String fileName)
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
    }
}
