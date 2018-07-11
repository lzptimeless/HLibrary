using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace H.BookLibrary
{
    public class DownloadService
    {
        #region fields

        #endregion

        private DownloadService()
        { }

        #region properties
        #region Instance
        private static DownloadService _instance;
        public static DownloadService Instance
        {
            get
            {
                if (Volatile.Read(ref _instance) == null)
                {
                    var instance = new DownloadService();
                    Interlocked.CompareExchange(ref _instance, instance, null);
                }

                return _instance;
            }
        }
        #endregion
        #endregion

        #region events
        public event EventHandler<BookEventArgs> DownloadBookStart;
        public event EventHandler<BookEventArgs> DownloadBookStop;
        public event EventHandler<BookEventArgs> DownloadBookResume;
        public event EventHandler<BookEventArgs> DownloadBookCompleted;
        public event EventHandler<PageEventArgs> DownloadPageCompleted;
        #endregion

        #region public methods
        public Task<IHBookHeader> GetDownloadingBook()
        {
            throw new NotImplementedException();
        }

        public Task<BooksResult> GetBooksAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetCoverThumbnailAsync(Guid bookID)
        {
            throw new NotImplementedException();
        }

        public Task<BitmapImage> GetPageThumbnailAsync(Guid bookID, Guid pageID)
        {
            throw new NotImplementedException();
        }

        public Task DownloadAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public Task ResumeAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
