using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class IOWriteFailedException : Exception
    {
        public IOWriteFailedException(string msg, Exception inner)
            :base(msg,inner)
        { }
    }
}
