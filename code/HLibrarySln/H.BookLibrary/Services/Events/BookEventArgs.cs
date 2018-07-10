using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class BookEventArgs : EventArgs
    {
        public BookEventArgs(IHBook book,IHBookHeader header)
        {
            Book = book;
            Header = header;
        }

        public IHBook Book { get; private set; }
        public IHBookHeader Header { get; private set; }
    }
}
