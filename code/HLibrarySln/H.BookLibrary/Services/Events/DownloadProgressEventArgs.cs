using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class DownloadProgressEventArgs : EventArgs
    {
        public DownloadProgressEventArgs(int progressMax, int progressValue)
        {
            ProgressMax = progressMax;
            ProgressValue = progressValue;
        }

        public int ProgressMax { get; private set; }
        public int ProgressValue { get; private set; }
    }
}
