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
        #endregion

        #region constructors
        private CoverCacheService()
        {
            _lock = new AsyncOneManyLock();
            _dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cover_cache");
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
            return Task.FromResult<object>(null);
        }

        public async Task<Stream> GetCoverCopyAsync(Guid bookID)
        {
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");

            await _lock.WaitAsync(false);
            MemoryStream ms = null;
            try
            {
                string path = Path.Combine(_dir, GetCoverFileName(bookID));
                if (File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    {
                        if (fs.Length > int.MaxValue) throw new ApplicationException($"Cover file is too big: path={path}, len={fs.Length}");

                        ms = new MemoryStream((int)fs.Length);
                        await fs.CopyToAsync(ms);
                    }
                }
            }
            catch
            {
                if (ms != null) ms.Dispose();
                throw;
            }
            finally
            {
                _lock.Release();
            }

            return ms;
        }

        public async Task ReadCoverAsync(Guid bookID, bool useAsyncStream, Func<Stream, Task> readAction)
        {
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");
            if (readAction == null) throw new ArgumentNullException("readAction");

            await _lock.WaitAsync(false);
            try
            {
                string path = Path.Combine(_dir, GetCoverFileName(bookID));
                if (File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsyncStream))
                    {
                        await readAction(fs);
                    }
                }
                else await readAction(null);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Stream> GetThumbnailCopyAsync(Guid bookID)
        {
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");

            await _lock.WaitAsync(false);
            MemoryStream ms = null;
            try
            {
                string path = Path.Combine(_dir, GetThumbnailFileName(bookID));
                if (File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    {
                        if (fs.Length > int.MaxValue) throw new ApplicationException($"Cover file is too big: path={path}, len={fs.Length}");

                        ms = new MemoryStream((int)fs.Length);
                        await fs.CopyToAsync(ms);
                    }
                }
            }
            catch
            {
                if (ms != null) ms.Dispose();
                throw;
            }
            finally
            {
                _lock.Release();
            }

            return ms;
        }

        public async Task ReadThumbnailAsync(Guid bookID, bool useAsyncStream, Func<Stream, Task> readAction)
        {
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");
            if (readAction == null) throw new ArgumentNullException("readAction");

            await _lock.WaitAsync(false);
            try
            {
                string path = Path.Combine(_dir, GetThumbnailFileName(bookID));
                if (File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsyncStream))
                    {
                        await readAction(fs);
                    }
                }
                else await readAction(null);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> SetCoverAsync(Guid bookID, Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");

            await _lock.WaitAsync(true);
            EnsureDirectoryExist();

            bool result;
            try
            {
                string path = Path.Combine(_dir, GetCoverFileName(bookID));
                if (File.Exists(path)) result = false;
                else
                {
                    using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        await stream.CopyToAsync(fs);
                    }
                    result = true;
                }
            }
            finally
            {
                _lock.Release();
            }
            return result;
        }

        public async Task<bool> SetThumbnailAsync(Guid bookID, Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");

            await _lock.WaitAsync(true);
            EnsureDirectoryExist();

            bool result;
            try
            {
                string path = Path.Combine(_dir, GetThumbnailFileName(bookID));
                if (File.Exists(path)) result = false;
                else
                {
                    using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        await stream.CopyToAsync(fs);
                    }
                    result = true;
                }
            }
            finally
            {
                _lock.Release();
            }
            return result;
        }

        public async Task<bool> ContainsCoverAsync(Guid bookID)
        {
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");

            await _lock.WaitAsync(false);
            bool result;
            try
            {
                string path = Path.Combine(_dir, GetCoverFileName(bookID));
                result = File.Exists(path);
            }
            finally
            {
                _lock.Release();
            }
            return result;
        }

        public async Task<bool> ContainsThumbnailAsync(Guid bookID)
        {
            if (bookID == Guid.Empty) throw new ArgumentException("bookID can not be empty");

            await _lock.WaitAsync(false);
            bool result;
            try
            {
                string path = Path.Combine(_dir, GetThumbnailFileName(bookID));
                result = File.Exists(path);
            }
            finally
            {
                _lock.Release();
            }
            return result;
        }
        #endregion

        #region private methods
        private void EnsureDirectoryExist()
        {
            if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
        }

        private static string GetCoverFileName(Guid bookID)
        {
            return $"{bookID.ToString("D")}.cover";
        }

        private static string GetThumbnailFileName(Guid bookID)
        {
            return $"{bookID.ToString("D")}.thumbnail";
        }
        #endregion
    }
}
