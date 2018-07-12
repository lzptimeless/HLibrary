using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class PagesResult
    {
        public PagesResult(int offset, int count, int total, PageResult[] pages)
        {
            Offset = offset;
            Count = count;
            Total = total;
            Pages = pages;
        }

        public int Offset { get; private set; }
        public int Count { get; private set; }
        public int Total { get; private set; }
        public PageResult[] Pages { get; private set; }
    }

    public class PageResult
    {
        public PageResult(IHBookHeader book, IHPageHeader page)
        {
            Book = book;
            Page = page;
        }
        public IHBookHeader Book { get; private set; }
        public IHPageHeader Page { get; private set; }
    }
}
