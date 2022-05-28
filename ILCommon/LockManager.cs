using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ILCommon
{
    public class LockManager
    {
        bool didILock = false;
        private readonly String LockFile = ".lck"; 

        public bool AcquireLock()
        {
            if (!File.Exists(LockFile))
            {
                try
                {
                    using (File.Create(LockFile))
                    {
                        didILock = true;
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        public void ReleaseLock()
        {
            if (didILock && File.Exists(LockFile))
            {
                try
                {
                    File.Delete(LockFile);
                }
                catch
                {
                }
            }
        }
    }
}
