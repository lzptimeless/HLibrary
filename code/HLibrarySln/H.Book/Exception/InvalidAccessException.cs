using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class InvalidAccessException : Exception
    {
        public InvalidAccessException(string msg)
            : base(msg, null)
        { }
    }
}
