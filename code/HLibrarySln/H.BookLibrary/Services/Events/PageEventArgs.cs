using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace H.BookLibrary
{
    public class PageEventArgs : EventArgs
    {
        public PageEventArgs(IHBook book, IHPageHeader pageHeader, ImageSource thumbnail, ImageSource content)
        {
            Book = book;
            Header = pageHeader;
            Thumbnail = thumbnail;
            Content = content;
        }

        public IHBook Book { get; private set; }
        public IHPageHeader Header { get; private set; }
        public ImageSource Thumbnail { get; private set; }
        public ImageSource Content { get; private set; }
    }
}
