using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class BookNotFoundException:Exception
    {
        public BookNotFoundException(string msg)
            :base(msg,null)
        { }
    }
}
