﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Book
{
    public class HBook : IHBook
    {
        #region fields
        private AsyncOneManyLockEx _lock = new AsyncOneManyLockEx(10);
        private HBookMode _mode;
        private Stream _stream;
        private int _pageHeaderListCount;
        private HMetadataBookHeader _headerMetadata = new HMetadataBookHeader();
        private HMetadataBookCover _coverMetadata = new HMetadataBookCover();
        private HMetadataPageCollection _pages = new HMetadataPageCollection();

        private HBookAccess _access;
        private bool CanAccessCover() { return _access != HBookAccess.Header; }
        private bool CanAccessPage() { return _access == HBookAccess.All; }
        private Exception CreateRequireCoverAccessEx() { return new InvalidAccessException($"Invalid access:access={_access}, request={HBookAccess.HeaderAndCover}|{HBookAccess.All}"); }
        private Exception CreateRequirePageAccessEx() { return new InvalidAccessException($"Invalid access:access={_access}, request={HBookAccess.All}"); }

        private int _isDisposed;
        private bool IsDisposed() { return Volatile.Read(ref _isDisposed) == 1; }
        private bool MakeDisposed() { return Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0; }
        private Exception CreateDisposedEx() { return new ObjectDisposedException("HBook", "HBook has been disposed"); }

        private int _isInitialized;
        private Exception _initEx;
        private bool IsInitialized() { return Volatile.Read(ref _isInitialized) == 1; }
        private bool MakeInitialized() { return Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0; }
        private bool IsInitFailed() { return Volatile.Read(ref _initEx) != null; }
        private Exception CreateInitFailedEx() { return new InitException("HBook load or create failed", Volatile.Read(ref _initEx)); }

        private Exception _ioWriteEx;
        private bool IsIOWriteFailed() { return Volatile.Read(ref _ioWriteEx) != null; }
        private Exception CreateIOWriteFailedEx() { return new IOWriteFailedException("A IO write error ocurred, data maybe damaged", Volatile.Read(ref _ioWriteEx)); }
        #endregion
        /// <summary>
        /// 打开或创建<see cref="HBook"/>
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="mode">打开或创建</param>
        /// <param name="access">访问内容，如果是创建文件则忽略这个参数，如果是打开文件则影响可操作的内容和初始化速度，可操作内容越少初始化速度越快</param>
        /// <param name="pageHeaderListCount">创建书时设置的页头列表数量，一个页头列表数包含1024个页头，如果是打开文件则忽略这个参数</param>
        public HBook(string path, HBookMode mode, HBookAccess access, byte pageHeaderListCount)
        {
            if (mode != HBookMode.OpenOrCreate)
                _mode = mode;
            else
                _mode = File.Exists(path) ? HBookMode.Open : HBookMode.Create;

            if (_mode == HBookMode.Create)
            {
                if (pageHeaderListCount < 1) throw new ArgumentOutOfRangeException("pageHeaderListCount", $"value={pageHeaderListCount}, range=[1,255]");

                _stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 2048, true);
                _access = HBookAccess.All;// 创建文件忽略access参数，直接用All
                _pageHeaderListCount = pageHeaderListCount;
            }
            else if (_mode == HBookMode.Open)
            {
                _stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 2048, true);
                _access = access;
                _pageHeaderListCount = 0;// 打开文件忽略pageHeaderListCount参数
            }
            else
                throw new ArgumentOutOfRangeException("mode", $"Not supported mode:{_mode}");
        }

        ~HBook()
        {
            Dispose();
        }

        private async Task InitIfNeedAsync(string callerFilePath, string callerName)
        {
            // 由于这个函数调用频繁在获取独占锁之前检测一次状态增加效率
            if (IsInitialized()) return;
            if (IsDisposed()) throw CreateDisposedEx();
            if (IsInitFailed()) throw CreateInitFailedEx();

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitialized()) return;
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsInitFailed()) throw CreateInitFailedEx();

                if (_mode == HBookMode.Create)
                    await CreateAsync();
                else if (_mode == HBookMode.Open)
                {
                    await LoadAsync();
                }
                else
                    throw new NotSupportedException($"Not supported mode:{_mode}");

                MakeInitialized();
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref _initEx, ex, null);
                throw ex;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        private async Task LoadAsync()
        {
            int readedLen = 0;
            int cacheLen = !CanAccessCover() ? HMetadataConstant.BookCoverPosition : (!CanAccessPage() ? HMetadataConstant.PageHeaderListCountPosition : (HMetadataConstant.FirstPageHeaderListEndPosition + 1));
            using (var cacheStream = await CreateMemoryCache(_stream, cacheLen))
            {
                // 验证文件头
                byte[] startCode = new byte[HMetadataConstant.StartCode.Length];
                readedLen = await cacheStream.ReadAsync(startCode, 0, startCode.Length);
                if (!startCode.SequenceEqual(HMetadataConstant.StartCode))
                {
                    _stream.Dispose();
                    _stream = null;
                    throw new InvalidDataException("StartCode error, this is not a HBook");
                }
                // 读取头
                await _headerMetadata.LoadAsync(cacheStream);
                // 读取封面
                if (CanAccessCover()) await _coverMetadata.LoadAsync(cacheStream);
                // 读取页面头
                if (CanAccessPage())
                {
                    // 验证当前读取位置是否符合预期
                    if (cacheStream.Position != HMetadataConstant.PageHeaderListCountPosition)
                        throw new InvalidDataException("Cover metadata not readed expect length");
                    // 读取页头列表数量
                    _pageHeaderListCount = cacheStream.ReadByte();
                    if (_pageHeaderListCount < 1) throw new InvalidDataException($"Page header list count error:value={_pageHeaderListCount}, range=[1,255]");
                    // 读取第一页头列表
                    await ReadPageHeadersAsync(cacheStream);
                    if (_pageHeaderListCount > 1)
                    {
                        // 读取后续页头列表
                        int otherHeadersLen = HMetadataConstant.PageHeaderListLength * (_pageHeaderListCount - 1);
                        using (var otherHeadersCacheS = await CreateMemoryCache(_stream, otherHeadersLen))
                        {
                            await ReadPageHeadersAsync(otherHeadersCacheS);
                        }
                    }
                }// if (access == HBookAccess.All)
            }// using cacheStream
        }

        private async Task CreateAsync()
        {
            // 初始化
            _headerMetadata.ID = Guid.NewGuid();
            _headerMetadata.Version = 1;
            // 写入起始码
            await _stream.WriteAsync(HMetadataConstant.StartCode, 0, HMetadataConstant.StartCode.Length);
            // 存储头
            await _headerMetadata.SaveAsync(_stream, null, 0);
            // 存储封面
            await _coverMetadata.SaveAsync(_stream, null, 0);
            // 写入页头列表数
            if (_pageHeaderListCount < 1 || _pageHeaderListCount > byte.MaxValue)
                throw new ApplicationException($"Page header list count error:value={_pageHeaderListCount}, range=[1,255]");

            await _stream.WriteByteAsync((byte)_pageHeaderListCount);
            // 在页头列表区域填充空数据
            int totalPageHeaderListLen = _pageHeaderListCount * HMetadataConstant.PageHeaderListLength;
            await _stream.FillAsync(HMetadataConstant.CCFlag, totalPageHeaderListLen);
        }

        public async Task<IHBookHeader> GetHeaderAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(false, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                return new HBookHeader(_headerMetadata);
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<IHBookHeader> SetHeaderAsync(HBookHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var metadata = _headerMetadata;
                var fs = metadata.FileStatus;
                var selected = header.Selected;
                // 检测当前属性是否符合预期
                var staleStatusEx = ExceptionFactory.CreateStaleStatusEx("Book header has changed");
                if (selected.HasFlag(HBookHeaderFieldSelections.IetfLanguageTag) && !FieldEqual(header.PreIetfLanguageTag, metadata.IetfLanguageTag)) throw staleStatusEx;
                if (selected.HasFlag(HBookHeaderFieldSelections.Names) && !FieldEqual(header.PreNames, metadata.Names)) throw staleStatusEx;
                if (selected.HasFlag(HBookHeaderFieldSelections.Artists) && !FieldEqual(header.PreArtists, metadata.Artists)) throw staleStatusEx;
                if (selected.HasFlag(HBookHeaderFieldSelections.Groups) && !FieldEqual(header.PreGroups, metadata.Groups)) throw staleStatusEx;
                if (selected.HasFlag(HBookHeaderFieldSelections.Series) && !FieldEqual(header.PreSeries, metadata.Series)) throw staleStatusEx;
                if (selected.HasFlag(HBookHeaderFieldSelections.Categories) && !FieldEqual(header.PreCategories, metadata.Categories)) throw staleStatusEx;
                if (selected.HasFlag(HBookHeaderFieldSelections.Characters) && !FieldEqual(header.PreCharacters, metadata.Characters)) throw staleStatusEx;
                if (selected.HasFlag(HBookHeaderFieldSelections.Tags) && !FieldEqual(header.PreTags, metadata.Tags)) throw staleStatusEx;

                // 更新
                if (selected.HasFlag(HBookHeaderFieldSelections.IetfLanguageTag)) metadata.IetfLanguageTag = header.IetfLanguageTag;
                if (selected.HasFlag(HBookHeaderFieldSelections.Names)) metadata.Names = header.Names;
                if (selected.HasFlag(HBookHeaderFieldSelections.Artists)) metadata.Artists = header.Artists;
                if (selected.HasFlag(HBookHeaderFieldSelections.Groups)) metadata.Groups = header.Groups;
                if (selected.HasFlag(HBookHeaderFieldSelections.Series)) metadata.Series = header.Series;
                if (selected.HasFlag(HBookHeaderFieldSelections.Categories)) metadata.Categories = header.Categories;
                if (selected.HasFlag(HBookHeaderFieldSelections.Characters)) metadata.Characters = header.Characters;
                if (selected.HasFlag(HBookHeaderFieldSelections.Tags)) metadata.Tags = header.Tags;

                // 保存
                _stream.Seek(fs.Position, SeekOrigin.Begin);
                await metadata.SaveAsync(_stream, null, 0);

                return new HBookHeader(_headerMetadata);
            }
            catch (IOException ioEx)
            {
                Interlocked.CompareExchange(ref _ioWriteEx, ioEx, null);
                throw ioEx;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task ReadCoverAsync(Func<Stream, Task> readAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessCover()) throw CreateRequireCoverAccessEx();

                var appendix = _coverMetadata.GetImage();
                if (appendix == null)
                {
                    await readAction.Invoke(null);
                    return;
                }

                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await readAction.Invoke(partStream);
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<Stream> GetCoverCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessCover()) throw CreateRequireCoverAccessEx();

                var appendix = _coverMetadata.GetImage();
                if (appendix == null)
                    return null;

                MemoryStream memStream = new MemoryStream(appendix.Length);
                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await partStream.CopyToAsync(memStream);

                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task SetCoverAsync(Stream thumb, Stream cover, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            if (thumb != null && (thumb.Length == 0 || thumb.Length > int.MaxValue))
                throw new ArgumentException($"thumb is too big:expected=[1,{int.MaxValue}], value={thumb.Length}", "thumb");

            if (cover != null && (cover.Length == 0 || cover.Length > int.MaxValue))
                throw new ArgumentException($"cover is too big:expected=[1,{int.MaxValue}], value={cover.Length}", "cover");

            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessCover()) throw CreateRequireCoverAccessEx();

                List<Stream> appendixes = new List<Stream>();
                if (thumb != null) appendixes.Add(thumb);
                if (cover != null) appendixes.Add(cover);

                _coverMetadata.HasThumbnail = thumb != null;
                _coverMetadata.HasImage = cover != null;

                _stream.Seek(_coverMetadata.FileStatus.Position, SeekOrigin.Begin);
                await _coverMetadata.SaveAsync(_stream, appendixes.ToArray(), 0);
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task ReadCoverThumbnailAsync(Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessCover()) throw CreateRequireCoverAccessEx();

                var appendix = _coverMetadata.GetThumbnail();
                if (appendix == null)
                {
                    await readerAction.Invoke(null);
                    return;
                }

                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await readerAction.Invoke(partStream);
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<Stream> GetCoverThumbnailCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessCover()) throw CreateRequireCoverAccessEx();

                var appendix = _coverMetadata.GetThumbnail();
                if (appendix == null)
                    return null;

                MemoryStream memStream = new MemoryStream(appendix.Length);
                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await partStream.CopyToAsync(memStream);

                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<IHPageHeader[]> GetPageHeadersAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(false, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();

                return _pages.GetAvailablePageHeaders();
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<bool> ReadPageAsync(Guid id, Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();

                var page = _pages[id];
                if (page == null) return false;

                var metadata = await GetPageContent(_stream, page);
                var fileStatus = metadata.FileStatus;
                var appendix = metadata.GetImage();
                if (appendix == null)
                {
                    await readerAction.Invoke(null);
                    return true;
                }

                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await readerAction.Invoke(partStream);

                return true;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<Stream> GetPageCopyAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();

                var page = _pages[id];
                if (page == null) throw ExceptionFactory.CreatePageNotFoundEx(id);

                var metadata = await GetPageContent(_stream, page);
                var fileStatus = metadata.FileStatus;
                var appendix = metadata.GetImage();
                if (appendix == null)
                    return null;

                MemoryStream memStream = new MemoryStream(appendix.Length);
                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await partStream.CopyToAsync(memStream);

                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<bool> ReadThumbnailAsync(Guid id, Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();

                var page = _pages[id];
                if (page == null) return false;

                var metadata = await GetPageContent(_stream, page);
                var fileStatus = metadata.FileStatus;
                var appendix = metadata.GetThumbnail();
                if (appendix == null)
                {
                    await readerAction.Invoke(null);
                    return true;
                }

                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await readerAction.Invoke(partStream);

                return true;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<Stream> GetThumbnailCopyAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();

                var page = _pages[id];
                if (page == null) throw ExceptionFactory.CreatePageNotFoundEx(id);

                var metadata = await GetPageContent(_stream, page);
                var fileStatus = metadata.FileStatus;
                var appendix = metadata.GetThumbnail();
                if (appendix == null)
                    return null;

                MemoryStream memStream = new MemoryStream(appendix.Length);
                using (Stream partStream = CreatePartReadStream(_stream, appendix.Position, appendix.Length))
                    await partStream.CopyToAsync(memStream);

                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<IHPageHeader> AddPageAsync(HPageHeaderSetting header, Stream thumbnail, Stream content, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            if (content != null && content.Length > int.MaxValue)
                throw new ArgumentException($"content is too big:max={int.MaxValue}, value={content.Length}", "content");

            if (thumbnail != null && thumbnail.Length > int.MaxValue)
                throw new ArgumentException($"thumbnail is too big:max={int.MaxValue}, value={thumbnail.Length}", "thumbnail");

            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();
                // 写入页图像
                _stream.Seek(0, SeekOrigin.End);// 页面内容直接增加到文件末尾

                HMetadataPageContent contentMetadata = new HMetadataPageContent();
                contentMetadata.HasThumbnail = thumbnail != null;
                contentMetadata.HasImage = content != null;

                List<Stream> appendixes = new List<Stream>();
                if (thumbnail != null) appendixes.Add(thumbnail);
                if (content != null) appendixes.Add(content);

                await contentMetadata.SaveAsync(_stream, appendixes.ToArray(), 0);

                // 准备写入页头的起始位置
                if (_pages.Count == 0)
                    _stream.Seek(HMetadataConstant.FirstPageHeaderListPosition, SeekOrigin.Begin);
                else
                {
                    var lastPageHeaderFs = _pages[_pages.Count - 1].HeaderMetadata.FileStatus;
                    long newPagePosition = lastPageHeaderFs.Position + lastPageHeaderFs.GetSpace();
                    long pageContentListPosition = HMetadataConstant.FirstPageHeaderListPosition + _pageHeaderListCount * HMetadataConstant.PageHeaderListLength;
                    if (pageContentListPosition - newPagePosition < HMetadataConstant.PageHeaderLength)
                        throw new ReserveSpaceNotEnoughException("PageHeader space not enough");

                    _stream.Seek(newPagePosition, SeekOrigin.Begin);
                }
                // 写入页头
                HMetadataPageHeader headerMetadata = new HMetadataPageHeader();
                headerMetadata.ID = Guid.NewGuid();
                headerMetadata.ContentPosition = contentMetadata.FileStatus.Position;

                if (header.Selected.HasFlag(HPageHeaderFieldSelections.Artist)) headerMetadata.Artist = header.Artist;
                if (header.Selected.HasFlag(HPageHeaderFieldSelections.Characters)) headerMetadata.Characters = header.Characters;
                if (header.Selected.HasFlag(HPageHeaderFieldSelections.Tags)) headerMetadata.Tags = header.Tags;

                await headerMetadata.SaveAsync(_stream, null, 0);
                // 添加页面信息到集合
                HMetadataPage pageMetadata = new HMetadataPage(headerMetadata, contentMetadata);
                if (!_pages.Add(pageMetadata))
                    throw new ApplicationException($"Unkown error, add page failed: id={headerMetadata.ID}");

                return new HPageHeader(headerMetadata);
            }
            catch (IOException ioEx)
            {
                Interlocked.CompareExchange(ref _ioWriteEx, ioEx, null);
                throw ioEx;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<bool> DeletePageAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();

                var page = _pages[id];
                if (page == null) return false;

                var headerMetadata = page.HeaderMetadata;
                var headerFS = headerMetadata.FileStatus;

                headerMetadata.IsDeleted = true;

                _stream.Seek(headerFS.Position, SeekOrigin.Begin);
                await headerMetadata.SaveAsync(_stream, null, 0);

                return true;
            }
            catch (IOException ioEx)
            {
                Interlocked.CompareExchange(ref _ioWriteEx, ioEx, null);
                throw ioEx;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<IHPageHeader> SetPageHeaderAsync(Guid id, HPageHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            await InitIfNeedAsync(callerFilePath, callerName);

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsDisposed()) throw CreateDisposedEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();
                if (!CanAccessPage()) throw CreateRequirePageAccessEx();

                var page = _pages[id];
                if (page == null) throw ExceptionFactory.CreatePageNotFoundEx(id);

                var metadata = page.HeaderMetadata;
                var fs = metadata.FileStatus;
                var selected = header.Selected;
                // 检测属性是否符合预期
                var staleStatusEx = ExceptionFactory.CreateStaleStatusEx($"Page header has changed: pageID={id}");
                if (selected.HasFlag(HPageHeaderFieldSelections.Artist) && !FieldEqual(header.PreArtist, metadata.Artist)) throw staleStatusEx;
                if (selected.HasFlag(HPageHeaderFieldSelections.Characters) && !FieldEqual(header.PreCharacters, metadata.Characters)) throw staleStatusEx;
                if (selected.HasFlag(HPageHeaderFieldSelections.Tags) && !FieldEqual(header.PreTags, metadata.Tags)) throw staleStatusEx;

                // 更新属性
                if (selected.HasFlag(HPageHeaderFieldSelections.Artist)) metadata.Artist = header.Artist;
                if (selected.HasFlag(HPageHeaderFieldSelections.Characters)) metadata.Characters = header.Characters;
                if (selected.HasFlag(HPageHeaderFieldSelections.Tags)) metadata.Tags = header.Tags;

                // 保存
                _stream.Seek(fs.Position, SeekOrigin.Begin);
                await metadata.SaveAsync(_stream, null, 0);

                return new HPageHeader(metadata);
            }
            catch (IOException ioEx)
            {
                Interlocked.CompareExchange(ref _ioWriteEx, ioEx, null);
                throw ioEx;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public void Dispose()
        {
            if (!MakeDisposed()) return;

            _stream?.Dispose();
            GC.SuppressFinalize(this);
        }
        #region private methods
        private async Task ReadPageHeadersAsync(Stream stream)
        {
            byte cc = 0;
            while (0 != (cc = await ReadNextControlCodeAsync(stream)))
            {
                // 移动读取位置到数据对起始位置
                stream.Seek(-2, SeekOrigin.Current);
                // 读取数据段
                if (cc == HMetadataControlCodes.PageHeader)
                {
                    HMetadataPageHeader pageHeaderMetadata = new HMetadataPageHeader();
                    await pageHeaderMetadata.LoadAsync(stream);
                    // 添加到集合
                    if (!_pages.Add(new HMetadataPage(pageHeaderMetadata, null)))
                        throw new InvalidDataException("Found duplicate id page");
                }
                else
                    throw new InvalidDataException($"Not support page header control code: {cc}");
            }// while (0 != (cc = ReadNextControlCode(cacheStream)))
        }

        private static string CreateReceiver(string filePath, string caller)
        {
            return $"{Path.GetFileName(filePath)}:{caller}";
        }

        /// <summary>
        /// 获取页面的页面内容，如果页面内容为null则自动读取
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="page">要获取内容的页面</param>
        /// <returns></returns>
        private static async Task<HMetadataPageContent> GetPageContent(Stream stream, HMetadataPage page)
        {
            if (page.ContentMetadata != null)
                return page.ContentMetadata;

            stream.Seek(page.HeaderMetadata.ContentPosition, SeekOrigin.Begin);
            HMetadataPageContent content = new HMetadataPageContent();
            await content.LoadAsync(stream);

            page.ContentMetadata = content;
            return content;
        }

        /// <summary>
        /// 读取下一个控制码
        /// </summary>
        /// <param name="stream">用以读取的Stream</param>
        /// <returns>控制码，0表示已经读到结尾了</returns>
        /// <exception cref="InvalidDataException">没有找到控制码标志<see cref="HMetadataConstant.CCFlag"/></exception>
        private static async Task<byte> ReadNextControlCodeAsync(Stream stream)
        {
            int readLen = 0;
            if (stream.Position >= stream.Length)
                return 0;

            // 假设接下来的两个字节就是控制码，直接读出
            byte[] two = new byte[2];
            readLen = await stream.ReadAsync(two, 0, two.Length);

            if (two[0] != HMetadataConstant.CCFlag)
                throw new InvalidDataException("Not found ControlCodeFlag");

            // 返回控制码或者0（初始值）
            if (readLen < 2 || two[1] != HMetadataConstant.CCFlag)
                return two[1];

            if (stream.Position >= stream.Length)
                return 0;

            // 控制码位置不确定，可能比较远，用Buffer减少IO请求次数
            byte[] buffer = new byte[1024];
            while (stream.Position < stream.Length)
            {
                readLen = await stream.ReadAsync(buffer, 0, buffer.Length);
                // 如果没有读取到数据则证明已经读完了，直接退出
                if (readLen == 0)
                    break;

                for (int i = 0; i < readLen; i++)
                {
                    if (buffer[i] != HMetadataConstant.CCFlag)
                    {
                        // 调整Position到控制码后的数据起始位
                        stream.Seek(-(readLen - 1 - i), SeekOrigin.Current);
                        return buffer[i];
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// 把<see cref="Stream"/>的一部分抽象成一个只读的新的<see cref="Stream"/>
        /// </summary>
        /// <param name="stream">原始的<see cref="Stream"/></param>
        /// <param name="partPosition">新<see cref="Stream"/>在原始<see cref="Stream"/>中的起始位置</param>
        /// <param name="partLength">新<see cref="Stream"/>的长度</param>
        /// <returns></returns>
        private static PartReadStream CreatePartReadStream(Stream stream, long partPosition, long partLength)
        {
            if (partPosition < 0 || partPosition >= stream.Length)
                throw new ArgumentOutOfRangeException("partPosition", $"origin stream length:{stream.Length}, partPosition:{partPosition}");

            if (partLength <= 0 || partLength > stream.Length - partPosition)
                throw new ArgumentOutOfRangeException("partLength", $"origin stream length:{stream.Length}, partPosition:{partPosition}, partLength:{partLength}");

            stream.Position = partPosition;
            PartReadStream partStream = new PartReadStream(stream, partPosition, partLength);
            return partStream;
        }

        /// <summary>
        /// 创建内存缓存，减少外部IO操作，只读，<see cref="MemoryStream"/>内部重载了<see cref="Stream.ReadAsync(byte[], int, int)"/>，内部同步执行读取操作，不会异步操作
        /// </summary>
        /// <param name="stream">外部数据流</param>
        /// <param name="len">缓存大小</param>
        /// <returns></returns>
        private static async Task<MemoryStream> CreateMemoryCache(Stream stream, int len)
        {
            byte[] buffer = new byte[len];
            int readLen = await stream.ReadAsync(buffer, 0, len);
            // 直接用buffer创建MemoryStream，MemoryStream内部会直接使用buffer而不会自己再创建新的内存空间
            return new MemoryStream(buffer, 0, readLen, false);
        }

        /// <summary>
        /// 比较两个字符串字段是否相等，这里null被认为与empty相等
        /// </summary>
        /// <param name="l">用于比较的字段1</param>
        /// <param name="r">用于比较的字段2</param>
        /// <returns>true：两个字段相等，false：两个字段不等</returns>
        private static bool FieldEqual(string l, string r)
        {
            if (string.IsNullOrEmpty(l) && string.IsNullOrEmpty(r)) return true;
            if (string.IsNullOrEmpty(l) || string.IsNullOrEmpty(r)) return false;

            return l.Equals(r);
        }

        /// <summary>
        /// 比较两个字符串数组是否相等，这里null和空数组被认为相等，字符串null与empty被认为相等
        /// </summary>
        /// <param name="l">用于比较的字段1</param>
        /// <param name="r">用于比较的字段2</param>
        /// <returns>true：两个字段相等，false：两个字段不等</returns>
        private static bool FieldEqual(string[] l, string[] r)
        {
            if ((l == null || l.Length == 0) && (r == null || r.Length == 0)) return true;
            if ((l == null || l.Length == 0) || (r == null || r.Length == 0)) return false;
            if (l.Length != r.Length) return false;

            for (int i = 0; i < l.Length; i++)
            {
                if (!FieldEqual(l[i], r[i])) return false;
            }

            return true;
        }
        #endregion
    }

    public interface IHBook : IDisposable
    {
        /// <summary>
        /// 从文件中加载
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">已经初始化了</exception>
        /// <exception cref="InvalidDataException">文件起始标识错误，或发现重复ID的页面，或发现不支持控制码</exception>
        //Task InitAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取头信息
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>头信息</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        Task<IHBookHeader> GetHeaderAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 修改头信息
        /// </summary>
        /// <param name="header">头信息</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>修改后的头信息</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="ReserveSpaceNotEnoughException">空间不足</exception> 
        /// <exception cref="StaleStatusException">书头在此之前已经发生了改变</exception>
        Task<IHBookHeader> SetHeaderAsync(HBookHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 读取封面
        /// </summary>
        /// <param name="readAction">读取<see cref="Action"/></param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作封面的权限</exception>
        Task ReadCoverAsync(Func<Stream, Task> readAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取封面副本
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>封面副本</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作封面的权限</exception>
        Task<Stream> GetCoverCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 读取封面缩略图
        /// </summary>
        /// <param name="readerAction">读取<see cref="Action"/></param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作封面的权限</exception>
        Task ReadCoverThumbnailAsync(Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取封面缩略图副本
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>封面缩略图副本</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作封面的权限</exception>
        Task<Stream> GetCoverThumbnailCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 设置封面
        /// </summary>
        /// <param name="thumb">封面缩略图</param>
        /// <param name="cover">封面</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns></returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作封面的权限</exception>
        Task SetCoverAsync(Stream thumb, Stream cover, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取所有页面头
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>页面头</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        Task<IHPageHeader[]> GetPageHeadersAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 读取页面图像，返回true时图像也有可能为null
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="readerAction">读取<see cref="Action"/></param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>返回true：读取成功，false：这个页面不存在</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        Task<bool> ReadPageAsync(Guid id, Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取页面图像副本
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>页面图像副本</returns>
        /// <exception cref="PageNotFoundException">没有找到这个页面</exception>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        Task<Stream> GetPageCopyAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 读取页面缩略图
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="readerAction">读取<see cref="Action"/></param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>ture：找到页面，false：没有找到页面</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        Task<bool> ReadThumbnailAsync(Guid id, Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取页面缩略图副本
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>页面缩略图副本</returns>
        /// <exception cref="PageNotFoundException">没有找到页面</exception>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        Task<Stream> GetThumbnailCopyAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 添加页面
        /// </summary>
        /// <param name="header">页面头</param>
        /// <param name="thumbnail">页面缩略图</param>
        /// <param name="content">页面图像</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>新的页面的头信息</returns>
        /// <exception cref="ArgumentNullException">content为null</exception>
        /// <exception cref="ArgumentException">content太大了，超出定义</exception>
        /// <exception cref="ArgumentException">thumbnail太大了，超出定义</exception>
        /// <exception cref="ApplicationException">未知异常导致添加失败</exception>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        /// <exception cref="ReserveSpaceNotEnoughException">空间不足</exception> 
        Task<IHPageHeader> AddPageAsync(HPageHeaderSetting header, Stream thumbnail, Stream content, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 删除页面
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>返回true：删除成功，false：找不到这个页面</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        Task<bool> DeletePageAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 修改页面头信息
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="header">头信息</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>修改后的页头</returns>
        /// <exception cref="PageNotFoundException">找不到这个页面</exception>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        /// <exception cref="InvalidAccessException">没有操作页面的权限</exception>
        /// <exception cref="ReserveSpaceNotEnoughException">空间不足</exception> 
        /// <exception cref="StaleStatusException">页头在此之前已经发生了改变</exception>
        Task<IHPageHeader> SetPageHeaderAsync(Guid id, HPageHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
    }
}
