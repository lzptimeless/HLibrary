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
        private Stream _stream;
        private HBookHeader _header = new HBookHeader();
        private HMetadataBookCover _coverMetadata = new HMetadataBookCover();
        private HMetadataPageCollection _pages = new HMetadataPageCollection();

        public IHBookHeader Header { get { return _header; } }
        public IReadOnlyList<IHPageHeader> PageHeaders { get { return _pages.Headers; } }

        public async Task LoadAsync(string path)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 1024, true);
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
            // 跳过封面图像数据
            _stream.Seek(_coverMetadata.ThumbnailLength + _coverMetadata.ImageLength, SeekOrigin.Current);
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
                    // 跳过图像数据
                    _stream.Seek(page.ContentMetadata.ThumbnailLength + page.ContentMetadata.ImageLength, SeekOrigin.Current);
                    // 添加到集合
                    _pages.Add(page);
                }
                else if (cc == HMetadataControlCodes.VirtualPage)
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
                    _stream.Seek(pageContent.ThumbnailLength + pageContent.ImageLength, SeekOrigin.Current);
                }
                else
                    throw new InvalidDataException($"Not support control code: {cc}");
            }// while (0 != (cc = ReadNextControlCode(_stream)))
        }

        public async Task CreateAsync(string path)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 1024, true);
            // 存储头
            await _header.Metadata.CreateAsync(_stream);
            // 存储封面
            await _coverMetadata.CreateAsync(_stream);
        }

        // void ReadCover()
        // Stream GetCoverCopy()
        // void ReadCoverThumbnail(Action<Stream> reader)
        // Stream GetCoverThumbnailCopy() 
        // void ReadPage(int index, Action<Stream> reader)
        // Stream GetPageCopy(int index)
        // void ReadThumbnail(int index, Action<Stream> reader)
        // Stream GetThumbnailCopy(int index)
        // void AddPage(int index, PageHeader header, Stream page, Stream thumbnial)
        // void DeletePage(int index)

        private static void UpdateSegment(HMetadataSegment segment, Stream stream)
        {
            if (segment.Position < 0)
                throw new ArgumentException("segment", $"Position error: expected=[0,{int.MaxValue}], value={segment.Position}");

            int space = segment.GetSpace();
            if (stream.Length < segment.Position + space)
                throw new ArgumentException("stream", $"stream is not contains old segment: segment-pos={segment.Position}, segment-space={space}, stream-len={stream.Length}");

            if (stream.Position != segment.Position)
                stream.Seek(segment.Position, SeekOrigin.Begin);

            segment.SaveAsync(stream, space);
        }

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
    }

    public interface IHBook
    {
        IHBookHeader Header { get; }
        IReadOnlyList<IHPageHeader> PageHeaders { get; }
    }
}
