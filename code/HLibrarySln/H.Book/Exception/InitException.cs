using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class InitException : Exception
    {
        public InitException(string msg, Exception innerEx)
            : base(msg, innerEx)
        { }
    }
}
