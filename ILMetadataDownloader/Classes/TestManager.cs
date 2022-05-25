using System;

namespace MetadataDownloader
{
    internal class TestManager
    {
        internal void RunTests ()
        {
            var dao = new DAO ();

            Console.WriteLine ("Test execution {0}",
                dao.HasBeenDownloaded (
                    new ILCommon.Model.MDownloadedFile () {
                        FileName = " 720p.mp4",
                        Length = 1682214819
                    })
                );


            Console.WriteLine ("Test execution {0}",
                dao.HasBeenDownloaded (
                    new ILCommon.Model.MDownloadedFile () {
                        FileName = " 720p.mp4",
                        Length = 1682214810
                    })
                );


        }
    }
}
