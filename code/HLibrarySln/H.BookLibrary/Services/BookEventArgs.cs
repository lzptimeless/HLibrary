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
        public BookEventArgs(IHBookHeader book)
        {
            Book = book;
        }

        public IHBookHeader Book { get; private set; }
    }
}
