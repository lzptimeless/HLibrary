using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class PageNotFoundException:Exception
    {
        public PageNotFoundException(string msg)
            :base(msg)
        { }

        public PageNotFoundException(string msg,Exception ex)
            :base(msg,ex)
        { }
    }
}
