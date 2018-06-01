using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class PageEventArgs:EventArgs
    {
        public PageEventArgs(IHBookHeader book,IHPageHeader page)
        {
            Book = book;
            Page = page;
        }

        public IHBookHeader Book { get; private set; }
        public IHPageHeader Page { get; private set; }
    }
}
