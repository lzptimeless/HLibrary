using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class StaleStatusException : Exception
    {
        public StaleStatusException(string msg)
            : base(msg, null)
        { }
    }
}
