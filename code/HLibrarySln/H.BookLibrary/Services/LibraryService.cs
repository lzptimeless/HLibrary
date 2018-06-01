using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace H.BookLibrary
{
    public class LibraryService
    {
        public Task<BooksResult> GetBooksAsync(Func<IHBookHeader, bool> filter, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task<PagesResult> GetPagesAsync(Func<IHPageHeader, bool> filter, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetCoverThumbnailAsync(Guid bookID)
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetCoverAsync(Guid bookID)
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetPageThumbnailAsync(Guid bookID, Guid pageID)
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetPageAsync(Guid bookID, Guid pageID)
        {
            throw new NotImplementedException();
        }
    }
}
