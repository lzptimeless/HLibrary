using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HBookPartStreamIsDisposedChangedArgs : EventArgs
    {
        public HBookPartStreamIsDisposedChangedArgs(bool isDisposed)
        {
            IsDisposed = isDisposed;
        }

        public bool IsDisposed { get; private set; }
    }
}
