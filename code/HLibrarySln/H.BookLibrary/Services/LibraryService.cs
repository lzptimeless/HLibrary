using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Threading;
using System.IO;

namespace H.BookLibrary
{
    public class LibraryService
    {
        #region fields
        private const string FileFilter = "*.hb";
        /// <summary>
        /// 书库文件夹路径
        /// </summary>
        private string _dir;
        /// <summary>
        /// 监控书库文件夹变化
        /// </summary>
        private FileSystemWatcher _fsw;
        /// <summary>
        /// 书集合
        /// </summary>
        private BookCollection _books = new BookCollection();
        #endregion

        private LibraryService()
        {
            _dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "books");
        }

        #region properties
        #region Instance
        private static LibraryService _instance;
        /// <summary>
        /// Get or set <see cref="Instance"/>
        /// </summary>
        public static LibraryService Instance
        {
            get
            {
                if (Volatile.Read(ref _instance) == null)
                {
                    var instance = new LibraryService();
                    Interlocked.CompareExchange(ref _instance, instance, null);
                }

                return _instance;
            }
        }
        #endregion
        #endregion

        #region event handles
        #region InitProgressChanged
        /// <summary>
        /// 初始化进度
        /// </summary>
        public event EventHandler<ProgressEventArgs> InitProgressChanged;

        private void OnInitProgressChanged(ProgressEventArgs e)
        {
            Volatile.Read(ref InitProgressChanged)?.Invoke(this, e);
        }
        #endregion
        #endregion

        #region public methods
        public async Task InitializeAsync()
        {
            await LoadBooksAsync();
            StartFileSystemWatcher();
        }

        public Task<BooksResult> GetBooksAsync(Func<IHBookHeader, bool> filter, int offset, int count)
        {
            var result = _books.GetBooks(filter, offset, count);
            return Task.FromResult(result);
        }

        public Task<PagesResult> GetPagesAsync(Func<IHPageHeader, bool> filter, int offset, int count)
        {
            var result = _books.GetPages(filter, offset, count);
            return Task.FromResult(result);
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
        #endregion

        #region private methods
        private async Task LoadBooksAsync()
        {
            var files = Directory.GetFiles(_dir, FileFilter);
            OnInitProgressChanged(new ProgressEventArgs(files.Length, 0));
            for (int i = 0; i < files.Length; i++)
            {
                string f = files[i];
                BookCacheItem cacheItem;
                try
                {
                    Uri path = new Uri(f, UriKind.RelativeOrAbsolute);
                    cacheItem = await LoadBookCacheItemAsync(path);
                }
                catch (Exception ex)
                {
                    Output.Print($"Load book failed: path={f}{Environment.NewLine}{ex}");
                    OnInitProgressChanged(new ProgressEventArgs(files.Length, i + 1));
                    continue;
                }

                _books.AddCacheItem(cacheItem);
                OnInitProgressChanged(new ProgressEventArgs(files.Length, i + 1));
            }
        }

        private void StartFileSystemWatcher()
        {
            _fsw = new FileSystemWatcher();
            _fsw.Path = _dir;
            _fsw.Filter = FileFilter;
            _fsw.IncludeSubdirectories = false;
            _fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _fsw.Created += _fsw_Created;
            _fsw.Deleted += _fsw_Deleted;
            _fsw.Renamed += _fsw_Renamed;
            _fsw.Changed += _fsw_Changed;

            _fsw.EnableRaisingEvents = true;
        }

        private void _fsw_Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                Uri path = new Uri(e.FullPath, UriKind.RelativeOrAbsolute);
                TryAddBookAsync(path).NoAwait();
            }
        }

        private void _fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Uri path = new Uri(e.FullPath, UriKind.RelativeOrAbsolute);
                TryAddOrUpdateBookAsync(path).NoAwait();
            }
        }

        private void _fsw_Renamed(object sender, RenamedEventArgs e)
        {
            Uri oldPath = new Uri(e.OldFullPath, UriKind.RelativeOrAbsolute);
            Uri newPath = new Uri(e.FullPath, UriKind.RelativeOrAbsolute);
            _books.SetCacheItemPath(oldPath, newPath);
        }

        private void _fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            Uri path = new Uri(e.FullPath, UriKind.RelativeOrAbsolute);
            _books.DeleteCacheItem(path);
        }

        private async Task<bool> TryAddBookAsync(Uri path)
        {
            if (path == null) throw new ArgumentNullException("path");

            if (_books.ContainsCacheItem(path)) return false;

            BookCacheItem cacheItem = null;
            try
            {
                cacheItem = await LoadBookCacheItemAsync(path);
            }
            catch
            {
                // 一些错误导致读取失败
            }

            if (cacheItem != null)
                return _books.TryAddCacheItem(cacheItem);
            else
                return false;
        }

        private async Task<bool> TryAddOrUpdateBookAsync(Uri path)
        {
            if (path == null) throw new ArgumentNullException("path");

            BookCacheItem cacheItem = null;
            try
            {
                cacheItem = await LoadBookCacheItemAsync(path);
            }
            catch
            {
                // 一些错误导致读取失败
            }

            if (cacheItem != null)
            {
                _books.AddOrUpdateCacheItem(cacheItem);
                return true;
            }
            else
                return false;
        }

        private async Task<BookCacheItem> LoadBookCacheItemAsync(Uri path)
        {
            BookCacheItem cacheItem = null;
            using (var book = new HBook(path.LocalPath, HBookMode.Open, HBookAccess.All, 0))
            {
                await book.InitAsync();
                var header = await book.GetHeaderAsync();
                var pageHeaders = await book.GetPageHeadersAsync();

                cacheItem = new BookCacheItem(header.ID, path, header, pageHeaders);
            }

            return cacheItem;
        }
        #endregion

        #region classes
        private class BookCacheItem
        {
            public BookCacheItem(Guid id, Uri path, IHBookHeader header, IEnumerable<IHPageHeader> pageHeaders)
            {
                if (header == null) throw new ArgumentNullException("header");
                if (path == null) throw new ArgumentNullException("path");

                ID = id;
                Path = path;
                Header = header;
                PageHeaders = new List<IHPageHeader>();
                if (pageHeaders != null)
                    PageHeaders.AddRange(pageHeaders);
            }
            /// <summary>
            /// 缓存书的头信息，用以查询使用
            /// </summary>
            public IHBookHeader Header { get; private set; }
            /// <summary>
            /// 缓存页面信息，用以查询使用
            /// </summary>
            public List<IHPageHeader> PageHeaders { get; private set; }
            /// <summary>
            /// 文件路径
            /// </summary>
            public Uri Path { get; set; }
            public Guid ID { get; private set; }
        }

        private class BookAccessItem
        {
            public BookAccessItem(Guid id, IHBook book)
            {
                ID = id;
                Book = book;
                Handles = new List<BookHandle>();
            }

            public Guid ID { get; private set; }
            /// <summary>
            /// 请求访问凭据
            /// </summary>
            public List<BookHandle> Handles { get; private set; }
            public IHBook Book { get; private set; }
        }

        /// <summary>
        /// 书缓存，线程安全
        /// </summary>
        private class BookCollection
        {
            private List<BookCacheItem> _cache = new List<BookCacheItem>();
            /// <summary>
            /// 书本访问接口相对item很少，所以单独存放增加查询效率
            /// </summary>
            private List<BookAccessItem> _access = new List<BookAccessItem>();
            private OneManyLock _lock = new OneManyLock();

            public BooksResult GetBooks(Func<IHBookHeader, bool> filter, int offset, int count)
            {
                List<IHBookHeader> bookHeaders = new List<IHBookHeader>();
                int index = 0, total = 0;

                _lock.Enter(false);
                try
                {
                    for (int i = 0; i < _cache.Count; i++)
                    {
                        var item = _cache[i];
                        if (filter != null && !filter.Invoke(item.Header))
                            continue;

                        ++total;
                        if (index >= offset && bookHeaders.Count < count)
                            bookHeaders.Add(item.Header);

                        ++index;
                    }
                }
                finally
                {
                    _lock.Leave();
                }

                return new BooksResult(offset, count, total, bookHeaders.ToArray());
            }

            public PagesResult GetPages(Func<IHPageHeader, bool> filter, int offset, int count)
            {
                List<PageResult> pages = new List<PageResult>();
                int index = 0, total = 0;

                _lock.Enter(false);
                try
                {
                    for (int i = 0; i < _cache.Count; i++)
                    {
                        var item = _cache[i];
                        for (int pi = 0; pi < item.PageHeaders.Count; pi++)
                        {
                            var ph = item.PageHeaders[pi];
                            if (filter != null && !filter.Invoke(ph)) continue;

                            ++total;
                            if (index >= offset && pages.Count < count)
                                pages.Add(new PageResult(item.Header, ph));

                            ++index;
                        }
                    }
                }
                finally
                {
                    _lock.Leave();
                }

                return new PagesResult(offset, count, total, pages.ToArray());
            }

            public IHBook GetAccessItem(Guid id)
            {
                IHBook book = null;
                _lock.Enter(false);
                var item = GetAccessItemInner(id);
                if (item != null)
                    book = item.Book;
                _lock.Leave();
                return book;
            }

            public void AddCacheItem(BookCacheItem item)
            {
                if (item == null) throw new ArgumentNullException("item");
                if (item.Path == null) throw new ArgumentException("The property 'Path' of item can not be null");
                if (item.Header == null) throw new ArgumentException("The property 'Header' of item can not be null");

                _lock.Enter(true);
                try
                {
                    if (GetCacheItemIndexInner(item.ID) >= 0)
                        throw new ArgumentException($"The cache item already exist: id={item.ID}, name={item.Header.Names.FirstOrDefault()}");

                    _cache.Add(item);
                }
                finally
                {
                    _lock.Leave();
                }
            }

            public bool TryAddCacheItem(BookCacheItem item)
            {
                if (item == null) throw new ArgumentNullException("item");
                if (item.Path == null) throw new ArgumentException("The property 'Path' of item can not be null");
                if (item.Header == null) throw new ArgumentException("The property 'Header' of item can not be null");

                bool result;
                _lock.Enter(true);
                if (GetCacheItemIndexInner(item.ID) >= 0)
                    result = false;
                else
                {
                    _cache.Add(item);
                    result = true;
                }
                _lock.Leave();
                return result;
            }

            public void AddOrUpdateCacheItem(BookCacheItem item)
            {
                if (item == null) throw new ArgumentNullException("item");
                if (item.Path == null) throw new ArgumentException("The property 'Path' of item can not be null");
                if (item.Header == null) throw new ArgumentException("The property 'Header' of item can not be null");

                _lock.Enter(true);
                var index = GetCacheItemIndexInner(item.ID);
                if (index >= 0) _cache[index] = item;
                else _cache.Add(item);
                _lock.Leave();
            }

            public bool SetCacheItemPath(Uri oldPath, Uri newPath)
            {
                if (oldPath == null) throw new ArgumentNullException("oldPath");
                if (newPath == null) throw new ArgumentNullException("newPath");

                bool result;
                _lock.Enter(true);
                var item = GetCacheItemInner(oldPath);
                if (item == null)
                    result = false;
                else
                {
                    item.Path = newPath;
                    result = true;
                }
                _lock.Leave();

                return result;
            }

            public bool ContainsCacheItem(Uri path)
            {
                if (path == null) throw new ArgumentNullException("path");

                bool result;
                _lock.Enter(false);
                result = GetCacheItemIndexInner(path) >= 0;
                _lock.Leave();
                return result;
            }

            public bool DeleteCacheItem(Uri path)
            {
                if (path == null) throw new ArgumentNullException("path");

                bool result;
                _lock.Enter(true);
                int index = GetCacheItemIndexInner(path);
                if (index < 0)
                    result = false;
                else
                {
                    _cache.RemoveAt(index);
                    result = true;
                }
                _lock.Leave();
                return result;
            }

            private int GetCacheItemIndexInner(Guid id)
            {
                for (int i = 0; i < _cache.Count; i++)
                {
                    if (_cache[i].Header.ID == id) return i;
                }

                return -1;
            }

            private int GetCacheItemIndexInner(Uri path)
            {
                for (int i = 0; i < _cache.Count; i++)
                {
                    if (_cache[i].Path.Equals(path)) return i;
                }

                return -1;
            }

            private BookCacheItem GetCacheItemInner(Guid id)
            {
                for (int i = 0; i < _cache.Count; i++)
                {
                    var item = _cache[i];
                    if (item.Header.ID == id) return item;
                }

                return null;
            }

            private BookCacheItem GetCacheItemInner(Uri path)
            {
                for (int i = 0; i < _cache.Count; i++)
                {
                    var item = _cache[i];
                    if (item.Path.Equals(path)) return item;
                }

                return null;
            }

            private int GetAccessItemIndexInner(Guid id)
            {
                for (int i = 0; i < _access.Count; i++)
                {
                    if (_access[i].ID == id) return i;
                }

                return -1;
            }

            private BookAccessItem GetAccessItemInner(Guid id)
            {
                for (int i = 0; i < _access.Count; i++)
                {
                    var item = _access[i];
                    if (item.ID == id) return item;
                }

                return null;
            }
        }
        #endregion
    }
}
