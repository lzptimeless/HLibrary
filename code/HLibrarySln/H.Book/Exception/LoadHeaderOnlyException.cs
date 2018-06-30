using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class LoadHeaderOnlyException : Exception
    {
        public LoadHeaderOnlyException(string msg)
            : base(msg)
        { }

        public LoadHeaderOnlyException(string msg, Exception ex)
            : base(msg, ex)
        { }
    }
}
