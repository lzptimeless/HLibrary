using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HBook : IHBook
    {
        private HBookStream _stream;
        private HBookHeader _header = new HBookHeader();
        private HMetadataBookCover _coverMetadata = new HMetadataBookCover();
        private HMetadataPageCollection _pages = new HMetadataPageCollection();

        public IHBookHeader Header { get { return _header; } }
        public IReadOnlyList<IHPageHeader> PageHeaders { get { return _pages.Headers; } }

        public async Task LoadAsync(string path)
        {
            _stream = new HBookStream(path, FileMode.Open);
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
            byte cc = await ReadNextControlCode(_stream);
            if (cc != HMetadataControlCodes.BookHeader)
                throw new InvalidDataException($"0 ControlCode is not Header: expected={HMetadataControlCodes.BookHeader}, value={cc}");

            await _header.Metadata.LoadAsync(_stream, false);
            // 读取封面
            cc = await ReadNextControlCode(_stream);
            if (cc == HMetadataControlCodes.BookCover)
                throw new InvalidDataException($"2 ControlCode is not Cover: expected={HMetadataControlCodes.BookCover}, value={cc}");

            await _coverMetadata.LoadAsync(_stream, false);
            // 读取页面
            while (0 != (cc = await ReadNextControlCode(_stream)))
            {
                if (cc == HMetadataControlCodes.PageHeader)
                {
                    HMetadataPage page = new HMetadataPage();
                    await page.HeaderMetadata.LoadAsync(_stream, false);
                    // 读取页面内容
                    cc = await ReadNextControlCode(_stream);
                    if (cc != HMetadataControlCodes.PageContent)
                        throw new InvalidDataException($"Not found page content: pageIndex={_pages.Count}, controlCode={cc}");

                    await page.ContentMetadata.LoadAsync(_stream, false);
                    // 添加到集合
                    _pages.Add(page);
                }
                else if (cc == HMetadataControlCodes.VirtualPageHeader)
                {
                    // 忽略虚拟页面
                    HMetadataVirtualPage virtualPage = new HMetadataVirtualPage();
                    await virtualPage.LoadAsync(_stream, false);
                }
                else if (cc == HMetadataControlCodes.DeletedPageHeader)
                {
                    // 忽略被删除的页面头
                    HMetadataDeletedPageHeader deletedPage = new HMetadataDeletedPageHeader();
                    await deletedPage.LoadAsync(_stream, false);
                }
                else if (cc == HMetadataControlCodes.PageContent)
                {
                    // 忽略被删除的页面内容或没有页头的内容
                    HMetadataPageContent pageContent = new HMetadataPageContent();
                    await pageContent.LoadAsync(_stream, false);
                }
                else
                    throw new InvalidDataException($"Not support control code: {cc}");
            }// while (0 != (cc = ReadNextControlCode(_stream)))
        }

        public async Task CreateAsync(string path)
        {
            _stream = new HBookStream(path, FileMode.CreateNew);

            int reserveLen = 0;

            // 初始化
            var headerMetadata = _header.Metadata;
            headerMetadata.ID = Guid.NewGuid();
            headerMetadata.Version = 1;
            // 存储头
            reserveLen = HMetadataConstant.GetDefaultReserveLength(HMetadataControlCodes.BookHeader);
            await headerMetadata.SaveAsync(_stream, null, reserveLen);
            // 存储封面
            reserveLen = HMetadataConstant.GetDefaultReserveLength(HMetadataControlCodes.BookCover);
            await _coverMetadata.SaveAsync(_stream, null, reserveLen);
        }

        public async Task SetHeader(HBookHeaderArgs header)
        {
            var metadata = _header.Metadata;
            var fs = metadata.FileStatus;
            // 更新
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.IetfLanguageTag)) metadata.IetfLanguageTag = header.IetfLanguageTag;
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.Names)) metadata.Names = header.Names;
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.Artists)) metadata.Artists = header.Artists;
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.Groups)) metadata.Groups = header.Groups;
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.Series)) metadata.Series = header.Series;
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.Categories)) metadata.Categories = header.Categories;
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.Characters)) metadata.Characters = header.Characters;
            if (header.Selected.HasFlag(HBookHeaderFieldSelections.Tags)) metadata.Tags = header.Tags;

            // 保存
            int space = fs.GetSpace();
            int segHeaderLen = fs.GetHeaderLength();
            int fieldsLen = metadata.GetFieldsLength();
            int reserveLen = checked(space - segHeaderLen - fieldsLen);
            if (reserveLen < 0)
                throw new ArgumentException($"header is too big: space={space}, fieldsLen={fieldsLen}, segHeaderLen={segHeaderLen}", "header");

            _stream.Seek(fs.Position, SeekOrigin.Current);
            await metadata.SaveAsync(_stream, null, reserveLen);
        }

        public async void ReadCover(Func<Stream, Task> readAction)
        {
            if (_coverMetadata.ImageLength == 0)
            {
                await readAction.Invoke(null);
                return;
            }

            using (Stream partStream = _stream.ReadPart(_coverMetadata.FileStatus.GetAppendixPosition() + _coverMetadata.ThumbnailLength, _coverMetadata.ImageLength))
                await readAction.Invoke(partStream);
        }

        public async Task<Stream> GetCoverCopy()
        {
            if (_coverMetadata.ImageLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(_coverMetadata.ImageLength);
            using (Stream partStream = _stream.ReadPart(_coverMetadata.FileStatus.GetAppendixPosition() + _coverMetadata.ThumbnailLength, _coverMetadata.ImageLength))
                await partStream.CopyToAsync(memStream);

            return memStream;
        }

        public async void ReadCoverThumbnail(Func<Stream, Task> readerAction)
        {
            if (_coverMetadata.ThumbnailLength == 0)
            {
                await readerAction.Invoke(null);
                return;
            }

            using (Stream partStream = _stream.ReadPart(_coverMetadata.FileStatus.GetAppendixPosition(), _coverMetadata.ThumbnailLength))
                await readerAction.Invoke(partStream);
        }

        public async Task<Stream> GetCoverThumbnailCopy()
        {
            if (_coverMetadata.ThumbnailLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(_coverMetadata.ThumbnailLength);
            using (Stream partStream = _stream.ReadPart(_coverMetadata.FileStatus.GetAppendixPosition(), _coverMetadata.ThumbnailLength))
                await partStream.CopyToAsync(memStream);

            return memStream;
        }

        public async void ReadPage(int index, Func<Stream, Task> readerAction)
        {
            var page = _pages[index];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ImageLength == 0)
            {
                await readerAction.Invoke(null);
                return;
            }

            using (Stream partStream = _stream.ReadPart(fileStatus.GetAppendixPosition() + metadata.ThumbnailLength, metadata.ImageLength))
                await readerAction.Invoke(partStream);
        }

        public async Task<Stream> GetPageCopy(int index)
        {
            var page = _pages[index];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ImageLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(metadata.ImageLength);
            using (Stream partStream = _stream.ReadPart(fileStatus.GetAppendixPosition() + metadata.ThumbnailLength, metadata.ImageLength))
                await partStream.CopyToAsync(memStream);

            return memStream;
        }

        public async void ReadThumbnail(int index, Func<Stream, Task> readerAction)
        {
            var page = _pages[index];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ThumbnailLength == 0)
            {
                await readerAction.Invoke(null);
                return;
            }

            using (Stream partStream = _stream.ReadPart(fileStatus.GetAppendixPosition(), metadata.ThumbnailLength))
                await readerAction.Invoke(partStream);
        }

        public async Task<Stream> GetThumbnailCopy(int index)
        {
            var page = _pages[index];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ThumbnailLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(metadata.ThumbnailLength);
            using (Stream partStream = _stream.ReadPart(fileStatus.GetAppendixPosition(), metadata.ThumbnailLength))
                await partStream.CopyToAsync(memStream);

            return memStream;
        }

        public async Task AddPage(int index, HPageHeaderArgs header, Stream content, Stream thumbnial)
        {
            ExceptionFactory.CheckArgNull("content", content);

            if (content.Length > int.MaxValue)
                throw new ArgumentException($"content is too big:max={int.MaxValue}, value={content.Length}", "content");

            if (thumbnial != null && thumbnial.Length > int.MaxValue)
                throw new ArgumentException($"thumbnial is too big:max={int.MaxValue}, value={thumbnial.Length}", "thumbnial");

            _stream.Seek(0, SeekOrigin.End);

            HMetadataPageHeader headerMetadata = new HMetadataPageHeader();
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
            _pages.Add(pageMetadata);
        }

        public async Task DeletePage(int index)
        {
            if (_pages.Count <= index)
                return;

            var page = _pages[index];
            var headerFS = page.HeaderMetadata.FileStatus;

            _pages.RemoveAt(index);
            _stream.Seek(headerFS.Position + 1, SeekOrigin.Begin);
            await _stream.WriteByteAsync(HMetadataControlCodes.DeletedPageHeader);
        }

        public async Task SetPageHeader(int index, HPageHeaderArgs header)
        {
            var page = _pages[index];
            var headerMetadata = page.HeaderMetadata;
            var headerFS = headerMetadata.FileStatus;

            // 更新属性
            if (header.Selected.HasFlag(HPageHeaderFieldSelections.Artist)) headerMetadata.Artist = header.Artist;
            if (header.Selected.HasFlag(HPageHeaderFieldSelections.Characters)) headerMetadata.Characters = header.Characters;
            if (header.Selected.HasFlag(HPageHeaderFieldSelections.Tags)) headerMetadata.Tags = header.Tags;

            // 保存
            int space = headerFS.GetSpace();
            int segHeaderLen = headerFS.GetHeaderLength();
            int fieldsLen = headerMetadata.GetFieldsLength();
            int reserveLen = checked(space - segHeaderLen - fieldsLen);
            if (reserveLen < 0)
                throw new ArgumentException($"header is too big: space={space}, fieldsLen={fieldsLen}, segHeaderLen={segHeaderLen}", "header");

            _stream.Seek(headerFS.Position, SeekOrigin.Begin);
            await headerMetadata.SaveAsync(_stream, null, reserveLen);
        }

        #region private methods
        /// <summary>
        /// 读取下一个控制码
        /// </summary>
        /// <param name="stream">用以读取的Stream</param>
        /// <returns>控制码，0表示已经读到结尾了</returns>
        /// <exception cref="InvalidDataException">没有找到控制码标志<see cref="HMetadataConstant.ControlCodeFlag"/></exception>
        private static async Task<byte> ReadNextControlCode(Stream stream)
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
        #endregion
    }

    public interface IHBook
    {
        IHBookHeader Header { get; }
        IReadOnlyList<IHPageHeader> PageHeaders { get; }

        Task LoadAsync(string path);
        Task CreateAsync(string path);
        Task SetHeader(HBookHeaderArgs header);
        void ReadCover(Func<Stream, Task> readAction);
        Task<Stream> GetCoverCopy();
        void ReadCoverThumbnail(Func<Stream, Task> readerAction);
        Task<Stream> GetCoverThumbnailCopy();
        void ReadPage(int index, Func<Stream, Task> readerAction);
        Task<Stream> GetPageCopy(int index);
        void ReadThumbnail(int index, Func<Stream, Task> readerAction);
        Task<Stream> GetThumbnailCopy(int index);
        Task AddPage(int index, HPageHeaderArgs header, Stream content, Stream thumbnial);
        Task DeletePage(int index);
        Task SetPageHeader(int index, HPageHeaderArgs header);
    }
}
