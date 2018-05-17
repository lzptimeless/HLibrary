using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Book
{
    public class HBook : IHBook
    {
        #region fields
        private OneManyLock _lock = new OneManyLock();
        private Stream _stream;
        private HBookHeader _header = new HBookHeader();
        private HMetadataBookCover _coverMetadata = new HMetadataBookCover();
        private HMetadataPageCollection _pages = new HMetadataPageCollection();
        #endregion

        #region properties
        public IHBookHeader Header { get { return _header; } }
        public IReadOnlyList<IHPageHeader> PageHeaders { get { return _pages.Headers; } }
        #endregion

        public async Task LoadAsync(string path)
        {
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
            await _header.Metadata.LoadAsync(_stream);
            // 读取封面
            await _coverMetadata.LoadAsync(_stream);
            // 读取页面
            byte cc = 0;
            while (0 != (cc = await ReadNextControlCode(_stream)))
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
                    _pages.Add(page);
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

        public async Task CreateAsync(string path)
        {
            _stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 2048, true);

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

        public async Task<bool> SetHeader(HBookHeaderSetting header)
        {
            var metadata = _header.Metadata;
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

        public async void ReadCover(Func<Stream, Task> readAction)
        {
            if (_coverMetadata.ImageLength == 0)
            {
                await readAction.Invoke(null);
                return;
            }

            using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition() + _coverMetadata.ThumbnailLength, _coverMetadata.ImageLength))
                await readAction.Invoke(partStream);
        }

        public async Task<Stream> GetCoverCopy()
        {
            if (_coverMetadata.ImageLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(_coverMetadata.ImageLength);
            using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition() + _coverMetadata.ThumbnailLength, _coverMetadata.ImageLength))
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

            using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition(), _coverMetadata.ThumbnailLength))
                await readerAction.Invoke(partStream);
        }

        public async Task<Stream> GetCoverThumbnailCopy()
        {
            if (_coverMetadata.ThumbnailLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(_coverMetadata.ThumbnailLength);
            using (Stream partStream = CreatePartReadStream(_stream, _coverMetadata.FileStatus.GetAppendixPosition(), _coverMetadata.ThumbnailLength))
                await partStream.CopyToAsync(memStream);

            return memStream;
        }

        public async void ReadPage(Guid id, Func<Stream, Task> readerAction)
        {
            var page = _pages[id];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ImageLength == 0)
            {
                await readerAction.Invoke(null);
                return;
            }

            using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition() + metadata.ThumbnailLength, metadata.ImageLength))
                await readerAction.Invoke(partStream);
        }

        public async Task<Stream> GetPageCopy(Guid id)
        {
            var page = _pages[id];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ImageLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(metadata.ImageLength);
            using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition() + metadata.ThumbnailLength, metadata.ImageLength))
                await partStream.CopyToAsync(memStream);

            return memStream;
        }

        public async void ReadThumbnail(Guid id, Func<Stream, Task> readerAction)
        {
            var page = _pages[id];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ThumbnailLength == 0)
            {
                await readerAction.Invoke(null);
                return;
            }

            using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition(), metadata.ThumbnailLength))
                await readerAction.Invoke(partStream);
        }

        public async Task<Stream> GetThumbnailCopy(Guid id)
        {
            var page = _pages[id];
            var metadata = page.ContentMetadata;
            var fileStatus = metadata.FileStatus;

            if (metadata.ThumbnailLength == 0)
                return null;

            MemoryStream memStream = new MemoryStream(metadata.ThumbnailLength);
            using (Stream partStream = CreatePartReadStream(_stream, fileStatus.GetAppendixPosition(), metadata.ThumbnailLength))
                await partStream.CopyToAsync(memStream);

            return memStream;
        }

        public async Task AddPage(Guid id, HPageHeaderSetting header, Stream content, Stream thumbnial)
        {
            ExceptionFactory.CheckArgNull("content", content);

            if (content.Length > int.MaxValue)
                throw new ArgumentException($"content is too big:max={int.MaxValue}, value={content.Length}", "content");

            if (thumbnial != null && thumbnial.Length > int.MaxValue)
                throw new ArgumentException($"thumbnial is too big:max={int.MaxValue}, value={thumbnial.Length}", "thumbnial");

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
            _pages.Add(pageMetadata);
        }

        public async Task DeletePage(Guid id)
        {
            var page = _pages[id];
            var headerFS = page.HeaderMetadata.FileStatus;

            _pages.Remove(page);
            _stream.Seek(headerFS.Position + 1, SeekOrigin.Begin);
            await _stream.WriteByteAsync(HMetadataControlCodes.DeletedPageHeader);
        }

        public async Task<bool> SetPageHeader(Guid index, HPageHeaderSetting header)
        {
            var page = _pages[index];
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
        IHBookHeader Header { get; }
        IReadOnlyList<IHPageHeader> PageHeaders { get; }

        Task LoadAsync(string path);
        Task CreateAsync(string path);
        /// <summary>
        /// 修改头信息
        /// </summary>
        /// <param name="header">头信息</param>
        /// <returns>true：成功，false：属性在修改前已经发生了改变</returns>
        Task<bool> SetHeader(HBookHeaderSetting header);
        void ReadCover(Func<Stream, Task> readAction);
        Task<Stream> GetCoverCopy();
        void ReadCoverThumbnail(Func<Stream, Task> readerAction);
        Task<Stream> GetCoverThumbnailCopy();
        void ReadPage(Guid id, Func<Stream, Task> readerAction);
        Task<Stream> GetPageCopy(Guid id);
        void ReadThumbnail(Guid id, Func<Stream, Task> readerAction);
        Task<Stream> GetThumbnailCopy(Guid id);
        Task AddPage(Guid id, HPageHeaderSetting header, Stream content, Stream thumbnial);
        Task DeletePage(Guid id);
        /// <summary>
        /// 修改页面头信息
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <param name="header">头信息</param>
        /// <returns>true：成功，false：属性在修改前已经发生了改变</returns>
        Task<bool> SetPageHeader(Guid id, HPageHeaderSetting header);
    }
}
