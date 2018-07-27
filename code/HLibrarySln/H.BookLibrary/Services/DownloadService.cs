using H.Book;
using System;
using System.Collections.Generic;
using System.IO;
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
        private string _dir;
        private DownloadCollection _items = new DownloadCollection();
        #endregion

        private DownloadService()
        {
            _dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
        }

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

        public Task DownloadAsync(string id)
        {
            EnsureDirectoryExist();
            string saveName = CreateSaveFileName(id);
            string savePath = Path.Combine(_dir, saveName);
            HitomiBookDownloader downloader = new HitomiBookDownloader(id, savePath);

            DownloadItem item = new DownloadItem(id, downloader);
            _items.Add(item);

            downloader.

            return downloader.DownloadAsync();
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

        #region private methods
        private void EnsureDirectoryExist()
        {
            if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
        }

        private static string CreateSaveFileName(string id)
        {
            return $"hitomi-{id}-{DateTime.Now.ToString("yyyy-MM-dd")}.hb";
        }
        #endregion

        #region classes
        private class DownloadItem
        {
            public DownloadItem(string id, HitomiBookDownloader downloader)
            {
                ID = id;
                Downloader = downloader;
            }

            public string ID { get; private set; }
            public HitomiBookDownloader Downloader { get; private set; }
        }

        private class DownloadCollection
        {
            #region fields
            private OneManyLock _lock = new OneManyLock();
            private List<DownloadItem> _items = new List<DownloadItem>();
            #endregion

            #region constructors

            #endregion

            #region properties

            #endregion

            #region public methods
            public void Add(DownloadItem item)
            {
                _lock.Enter(true);
                try
                {
                    if (FindIndex(item.ID) >= 0) throw new ArgumentException($"item already exist:id={item.ID}");

                    _items.Add(item);
                }
                finally
                {
                    _lock.Leave();
                }
            }
            #endregion

            #region private methods
            private int FindIndex(string id)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i].ID == id) return i;
                }

                return -1;
            }
            #endregion
        }
        #endregion
    }
}
