using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class CoverCacheService
    {
        #region fields
        private AsyncOneManyLock _lock;
        private string _dir;
        private string _dbPath;
        private Dictionary<Guid, string> _coverCache;
        private Dictionary<Guid, string> _thumbnailCache;
        #endregion

        #region constructors
        private CoverCacheService()
        {
            _lock = new AsyncOneManyLock();
            _dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cover_cache");
            _dbPath = Path.Combine(_dir, "cache.db");
            _coverCache = new Dictionary<Guid, string>();
            _thumbnailCache = new Dictionary<Guid, string>();
        }
        #endregion

        #region properties
        #region Instance
        private static CoverCacheService _instance;
        /// <summary>
        /// Get or set <see cref="Instance"/>
        /// </summary>
        public static CoverCacheService Instance
        {
            get
            {
                if (Volatile.Read(ref _instance) == null)
                {
                    var instance = new CoverCacheService();
                    Interlocked.CompareExchange(ref _instance, instance, null);
                }
                return _instance;
            }
        }
        #endregion
        #endregion

        #region public methods
        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public Stream GetCoverCopyAsync(Guid bookID)
        {
            throw new NotImplementedException();
        }

        public Task ReadCoverAsync(Func<Stream, Task> readAction)
        {
            throw new NotImplementedException();
        }

        public Stream GetThumbnailCopyAsync(Guid bookID)
        {
            throw new NotImplementedException();
        }

        public Task ReadThumbnailAsync(Func<Stream, Task> readAction)
        {
            throw new NotImplementedException();
        }

        public Task SetCoverAsync(Guid bookID, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task SetThumbnailAsync(Guid bookID, Stream stream)
        {
            throw new NotImplementedException();
        }

        public bool ContainsCover(Guid bookID)
        {
            throw new NotImplementedException();
        }

        public bool ContainsThumbnail(Guid bookID)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region private methods

        #endregion
    }
}
