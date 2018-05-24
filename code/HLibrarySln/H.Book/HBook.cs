﻿using System;
using System.Collections.Generic;
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
        private Stream _stream;
        private HMetadataBookHeader _headerMetadata = new HMetadataBookHeader();
        private HMetadataBookCover _coverMetadata = new HMetadataBookCover();
        private HMetadataPageCollection _pages = new HMetadataPageCollection();

        private Exception _loadEx;
        private Exception _createEx;
        private bool IsInitError() { return _stream == null || _loadEx != null || _createEx != null; }
        private Exception CreateInitErrorEx()
        {
            if (_stream == null) return new InitException("Not load or create", null);
            else return new InitException("HBook load or create failed", _loadEx ?? _createEx);
        }

        private Exception _ioWriteEx;
        private bool IsIOWriteFailed() { return _ioWriteEx != null; }
        private Exception CreateIOWriteFailedEx() { return new IOWriteFailedException("A IO write error ocurred, data maybe damaged", _ioWriteEx); }
        #endregion

        public async Task LoadAsync(string path, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (_stream != null) throw new ApplicationException("Can not load when contains data");

                _stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 2048, true);
                int readedLen = 0;
                // 验证文件头
                byte[] startCode = new byte[HMetadataConstant.StartCode.Length];
                readedLen = await _stream.ReadAsync(startCode, 0, startCode.Length);
                if (!startCode.SequenceEqual(HMetadataConstant.StartCode))
                {
                    _stream.Dispose();
                    _stream = null;
                    throw new InvalidDataException("StartCode error, this is not a HBook");
                }
                // 读取头
                await _headerMetadata.LoadAsync(_stream);
                // 读取封面
                await _coverMetadata.LoadAsync(_stream);
                // 读取页面
                byte cc = 0;
                while (0 != (cc = await ReadNextControlCodeAsync(_stream)))
                {
                    // 移动读取位置到数据对起始位置
                    _stream.Seek(-2, SeekOrigin.Current);
                    // 读取数据段
                    if (cc == HMetadataControlCodes.PageHeader)
                    {
                        HMetadataPage page = new HMetadataPage();
                        await page.HeaderMetadata.LoadAsync(_stream);
                        // 读取页面内容
                        await page.ContentMetadata.LoadAsync(_stream);
                        // 添加到集合
                        if (!_pages.Add(page))
                            throw new InvalidDataException("Found duplicate id page");
                    }
                    else if (cc == HMetadataControlCodes.VirtualPageHeader)
                    {
                        // 忽略虚拟页面
                        HMetadataVirtualPage virtualPage = new HMetadataVirtualPage();
                        await virtualPage.LoadAsync(_stream);
                    }
                    else if (cc == HMetadataControlCodes.DeletedPageHeader)
                    {
                        // 忽略被删除的页面头
                        HMetadataDeletedPageHeader deletedPage = new HMetadataDeletedPageHeader();
                        await deletedPage.LoadAsync(_stream);
                    }
                    else if (cc == HMetadataControlCodes.PageContent)
                    {
                        // 忽略被删除的页面内容或没有页头的内容
                        HMetadataPageContent pageContent = new HMetadataPageContent();
                        await pageContent.LoadAsync(_stream);
                    }
                    else
                        throw new InvalidDataException($"Not support control code: {cc}");
                }// while (0 != (cc = ReadNextControlCode(_stream)))
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref _loadEx, ex, null);
                throw ex;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task CreateAsync(string path, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (_stream != null) throw new ApplicationException("Can not create when contains data");

                _stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 2048, true);

                int reserveLen = 0;

                // 初始化
                _headerMetadata.ID = Guid.NewGuid();
                _headerMetadata.Version = 1;
                // 存储头
                reserveLen = HMetadataConstant.GetDefaultReserveLength(HMetadataControlCodes.BookHeader);
                await _headerMetadata.SaveAsync(_stream, null, reserveLen);
                // 存储封面
                reserveLen = HMetadataConstant.GetDefaultReserveLength(HMetadataControlCodes.BookCover);
                await _coverMetadata.SaveAsync(_stream, null, reserveLen);
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref _createEx, ex, null);
                throw ex;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<IHBookHeader> GetHeaderAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(false, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                return new HBookHeader(_headerMetadata);
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<bool> SetHeaderAsync(HBookHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var metadata = _headerMetadata;
                var fs = metadata.FileStatus;
                var selected = header.Selected;
                // 检测当前属性是否符合预期
                if (selected.HasFlag(HBookHeaderFieldSelections.IetfLanguageTag) && !FieldEqual(header.PreIetfLanguageTag, metadata.IetfLanguageTag)) return false;
                if (selected.HasFlag(HBookHeaderFieldSelections.Names) && !FieldEqual(header.PreNames, metadata.Names)) return false;
                if (selected.HasFlag(HBookHeaderFieldSelections.Artists) && !FieldEqual(header.PreArtists, metadata.Artists)) return false;
                if (selected.HasFlag(HBookHeaderFieldSelections.Groups) && !FieldEqual(header.PreGroups, metadata.Groups)) return false;
                if (selected.HasFlag(HBookHeaderFieldSelections.Series) && !FieldEqual(header.PreSeries, metadata.Series)) return false;
                if (selected.HasFlag(HBookHeaderFieldSelections.Categories) && !FieldEqual(header.PreCategories, metadata.Categories)) return false;
                if (selected.HasFlag(HBookHeaderFieldSelections.Characters) && !FieldEqual(header.PreCharacters, metadata.Characters)) return false;
                if (selected.HasFlag(HBookHeaderFieldSelections.Tags) && !FieldEqual(header.PreTags, metadata.Tags)) return false;

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
                int space = fs.GetSpace();
                int segHeaderLen = fs.GetHeaderLength();
                int fieldsLen = metadata.GetFieldsLength();
                int reserveLen = checked(space - segHeaderLen - fieldsLen);
                if (reserveLen < 0)
                    throw new ArgumentException($"header is too big: space={space}, fieldsLen={fieldsLen}, segHeaderLen={segHeaderLen}", "header");

                _stream.Seek(fs.Position, SeekOrigin.Current);
                await metadata.SaveAsync(_stream, null, reserveLen);

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

        public async void ReadCoverAsync(Func<Stream, Task> readAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                if (_coverMetadata.ImageLength == 0)
                {
                    await readAction.Invoke(null);
                    return;
                }

                using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition() + _coverMetadata.ThumbnailLength, _coverMetadata.ImageLength))
                    await readAction.Invoke(partStream);
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<Stream> GetCoverCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                if (_coverMetadata.ImageLength == 0)
                    return null;

                MemoryStream memStream = new MemoryStream(_coverMetadata.ImageLength);
                using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition() + _coverMetadata.ThumbnailLength, _coverMetadata.ImageLength))
                    await partStream.CopyToAsync(memStream);

                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async void ReadCoverThumbnailAsync(Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                if (_coverMetadata.ThumbnailLength == 0)
                {
                    await readerAction.Invoke(null);
                    return;
                }

                using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition(), _coverMetadata.ThumbnailLength))
                    await readerAction.Invoke(partStream);
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<Stream> GetCoverThumbnailCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                if (_coverMetadata.ThumbnailLength == 0)
                    return null;

                MemoryStream memStream = new MemoryStream(_coverMetadata.ThumbnailLength);
                using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition(), _coverMetadata.ThumbnailLength))
                    await partStream.CopyToAsync(memStream);

                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<IHPageHeader[]> GetPageHeadersAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(false, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                return _pages.GetPageHeaders();
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<bool> ReadPageAsync(Guid id, Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var page = _pages[id];
                if (page == null) return false;

                var metadata = page.ContentMetadata;
                var fileStatus = metadata.FileStatus;
                if (metadata.ImageLength == 0)
                {
                    await readerAction.Invoke(null);
                    return true;
                }

                using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition() + metadata.ThumbnailLength, metadata.ImageLength))
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
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var page = _pages[id];
                if (page == null) throw ExceptionFactory.CreatePageNotFoundEx(id);

                var metadata = page.ContentMetadata;
                var fileStatus = metadata.FileStatus;

                if (metadata.ImageLength == 0)
                    return null;

                MemoryStream memStream = new MemoryStream(metadata.ImageLength);
                using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition() + metadata.ThumbnailLength, metadata.ImageLength))
                    await partStream.CopyToAsync(memStream);

                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<bool> ReadThumbnailAsync(Guid id, Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var page = _pages[id];
                if (page == null) return false;

                var metadata = page.ContentMetadata;
                var fileStatus = metadata.FileStatus;

                if (metadata.ThumbnailLength == 0)
                {
                    await readerAction.Invoke(null);
                    return true;
                }

                using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition(), metadata.ThumbnailLength))
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
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var page = _pages[id];
                if (page == null) throw ExceptionFactory.CreatePageNotFoundEx(id);

                var metadata = page.ContentMetadata;
                var fileStatus = metadata.FileStatus;

                if (metadata.ThumbnailLength == 0)
                    return null;

                MemoryStream memStream = new MemoryStream(metadata.ThumbnailLength);
                using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition(), metadata.ThumbnailLength))
                    await partStream.CopyToAsync(memStream);

                return memStream;
            }
            finally
            {
                _lock.Release(wt);
            }
        }

        public async Task<Guid> AddPageAsync(HPageHeaderSetting header, Stream content, Stream thumbnial, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            ExceptionFactory.CheckArgNull("content", content);

            if (content.Length > int.MaxValue)
                throw new ArgumentException($"content is too big:max={int.MaxValue}, value={content.Length}", "content");

            if (thumbnial != null && thumbnial.Length > int.MaxValue)
                throw new ArgumentException($"thumbnial is too big:max={int.MaxValue}, value={thumbnial.Length}", "thumbnial");

            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                _stream.Seek(0, SeekOrigin.End);

                HMetadataPageHeader headerMetadata = new HMetadataPageHeader();
                headerMetadata.ID = Guid.NewGuid();

                if (header.Selected.HasFlag(HPageHeaderFieldSelections.Artist)) headerMetadata.Artist = header.Artist;
                if (header.Selected.HasFlag(HPageHeaderFieldSelections.Characters)) headerMetadata.Characters = header.Characters;
                if (header.Selected.HasFlag(HPageHeaderFieldSelections.Tags)) headerMetadata.Tags = header.Tags;

                int reserveLen = HMetadataConstant.GetDefaultReserveLength(HMetadataControlCodes.PageHeader);
                await headerMetadata.SaveAsync(_stream, null, reserveLen);

                HMetadataPageContent contentMetadata = new HMetadataPageContent();
                contentMetadata.ThumbnailLength = thumbnial != null ? (int)thumbnial.Length : 0;
                contentMetadata.ImageLength = content != null ? (int)content.Length : 0;

                List<Stream> appendixes = new List<Stream>();
                if (thumbnial != null) appendixes.Add(thumbnial);
                if (content != null) appendixes.Add(content);

                reserveLen = HMetadataConstant.GetDefaultReserveLength(HMetadataControlCodes.PageContent);
                await contentMetadata.SaveAsync(_stream, appendixes.ToArray(), reserveLen);

                HMetadataPage pageMetadata = new HMetadataPage(headerMetadata, contentMetadata);
                if (!_pages.Add(pageMetadata))
                    throw new ApplicationException($"Unkown error, add page failed: id={headerMetadata.ID}");

                return headerMetadata.ID;
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
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var page = _pages[id];
                if (page == null) return false;

                var headerFS = page.HeaderMetadata.FileStatus;

                _pages.Remove(page);
                _stream.Seek(headerFS.Position + 1, SeekOrigin.Begin);
                await _stream.WriteByteAsync(HMetadataControlCodes.DeletedPageHeader);

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

        public async Task<bool> SetPageHeaderAsync(Guid id, HPageHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            var wt = _lock.WaitAsync(true, Timeout.InfiniteTimeSpan, CancellationToken.None, CreateReceiver(callerFilePath, callerName));
            await wt;
            try
            {
                if (IsInitError()) throw CreateInitErrorEx();
                if (IsIOWriteFailed()) throw CreateIOWriteFailedEx();

                var page = _pages[id];
                if (page == null) throw ExceptionFactory.CreatePageNotFoundEx(id);

                var metadata = page.HeaderMetadata;
                var fs = metadata.FileStatus;
                var selected = header.Selected;
                // 检测属性是否符合预期
                if (selected.HasFlag(HPageHeaderFieldSelections.Artist) && !FieldEqual(header.PreArtist, metadata.Artist)) return false;
                if (selected.HasFlag(HPageHeaderFieldSelections.Characters) && !FieldEqual(header.PreCharacters, metadata.Characters)) return false;
                if (selected.HasFlag(HPageHeaderFieldSelections.Tags) && !FieldEqual(header.PreTags, metadata.Tags)) return false;

                // 更新属性
                if (selected.HasFlag(HPageHeaderFieldSelections.Artist)) metadata.Artist = header.Artist;
                if (selected.HasFlag(HPageHeaderFieldSelections.Characters)) metadata.Characters = header.Characters;
                if (selected.HasFlag(HPageHeaderFieldSelections.Tags)) metadata.Tags = header.Tags;

                // 保存
                int space = fs.GetSpace();
                int segHeaderLen = fs.GetHeaderLength();
                int fieldsLen = metadata.GetFieldsLength();
                int reserveLen = checked(space - segHeaderLen - fieldsLen);
                if (reserveLen < 0)
                    throw new ArgumentException($"header is too big: space={space}, fieldsLen={fieldsLen}, segHeaderLen={segHeaderLen}", "header");

                _stream.Seek(fs.Position, SeekOrigin.Begin);
                await metadata.SaveAsync(_stream, null, reserveLen);

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

        #region private methods
        private static string CreateReceiver(string filePath, string caller)
        {
            return $"{Path.GetFileName(filePath)}:{caller}";
        }

        /// <summary>
        /// 读取下一个控制码
        /// </summary>
        /// <param name="stream">用以读取的Stream</param>
        /// <returns>控制码，0表示已经读到结尾了</returns>
        /// <exception cref="InvalidDataException">没有找到控制码标志<see cref="HMetadataConstant.ControlCodeFlag"/></exception>
        private static async Task<byte> ReadNextControlCodeAsync(Stream stream)
        {
            int readLen = 0;
            if (stream.Position >= stream.Length)
                return 0;

            // 假设接下来的两个字节就是控制码，直接读出
            byte[] two = new byte[2];
            readLen = await stream.ReadAsync(two, 0, two.Length);

            if (two[0] != HMetadataConstant.ControlCodeFlag)
                throw new InvalidDataException("Not found ControlCodeFlag");

            // 返回控制码或者0（初始值）
            if (readLen < 2 || two[1] != HMetadataConstant.ControlCodeFlag)
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
                    if (buffer[i] != HMetadataConstant.ControlCodeFlag)
                    {
                        // 调整Position到控制码后的数据起始位
                        stream.Seek(readLen - i - 1, SeekOrigin.Current);
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

            PartReadStream partStream = new PartReadStream(stream, partPosition, partLength);
            return partStream;
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

    public interface IHBook
    {
        /// <summary>
        /// 从文件中加载
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">已经加载了数据</exception>
        /// <exception cref="InvalidDataException">文件起始标识错误，或发现重复ID的页面，或发现不支持控制码</exception>
        Task LoadAsync(string path, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 创建新的<see cref="HBook"/>文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"><see cref="HBook"/>已经加载了数据</exception>
        Task CreateAsync(string path, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
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
        /// <returns>true：成功，false：属性在修改前已经发生了改变</returns>
        /// <exception cref="ArgumentException">头信息太大了，超出预留空间</exception>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        Task<bool> SetHeaderAsync(HBookHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 读取封面
        /// </summary>
        /// <param name="readAction">读取<see cref="Action"/></param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        void ReadCoverAsync(Func<Stream, Task> readAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取封面副本
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>封面副本</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        Task<Stream> GetCoverCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 读取封面缩略图
        /// </summary>
        /// <param name="readerAction">读取<see cref="Action"/></param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        void ReadCoverThumbnailAsync(Func<Stream, Task> readerAction, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取封面缩略图副本
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>封面缩略图副本</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        Task<Stream> GetCoverThumbnailCopyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 获取所有页面头
        /// </summary>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>页面头</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
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
        Task<Stream> GetThumbnailCopyAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 添加页面
        /// </summary>
        /// <param name="header">页面头</param>
        /// <param name="content">页面图像</param>
        /// <param name="thumbnial">页面缩略图</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>新的页面的ID</returns>
        /// <exception cref="ArgumentNullException">content为null</exception>
        /// <exception cref="ArgumentException">content太大了，超出定义</exception>
        /// <exception cref="ArgumentException">thumbnial太大了，超出定义</exception>
        /// <exception cref="ApplicationException">未知异常导致添加失败</exception>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        Task<Guid> AddPageAsync(HPageHeaderSetting header, Stream content, Stream thumbnial, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 删除页面
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>返回true：删除成功，false：找不到这个页面</returns>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        Task<bool> DeletePageAsync(Guid id, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
        /// <summary>
        /// 修改页面头信息
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="header">头信息</param>
        /// <param name="callerFilePath">不需要设置</param>
        /// <param name="callerName">不需要设置</param>
        /// <returns>true：成功，false：属性在修改前已经发生了改变</returns>
        /// <exception cref="PageNotFoundException">找不到这个页面</exception>
        /// <exception cref="ArgumentException">头信息太大了，超出可用空间</exception>
        /// <exception cref="InitException">没有加载数据或，创建数据</exception>
        /// <exception cref="IOWriteFailedException">数据在之前的写入操作中可能已经损坏</exception>
        Task<bool> SetPageHeaderAsync(Guid id, HPageHeaderSetting header, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "");
    }
}
