using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class BooksResult
    {
        public BooksResult(int offset, int count, int total, IHBookHeader[] books)
        {
            Offset = offset;
            Count = count;
            Total = total;
            Books = books;
        }

        public int Offset { get; private set; }
        public int Count { get; private set; }
        public int Total { get; private set; }
        public IHBookHeader[] Books { get; private set; }
    }
}
