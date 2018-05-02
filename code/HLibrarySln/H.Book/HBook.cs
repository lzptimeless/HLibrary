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

        public void Load(string path)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            // 验证文件头
            byte[] startCode = new byte[HMetadataConstant.StartCode.Length];
            _stream.Read(startCode, 0, startCode.Length);
            if (!startCode.SequenceEqual(HMetadataConstant.StartCode))
            {
                _stream.Dispose();
                _stream = null;
                throw new InvalidDataException("StartCode error, this is not a HBook");
            }
            // 读取头
            byte cc = ReadNextControlCode(_stream);
            if (cc != HMetadataControlCodes.BookHeader)
                throw new InvalidDataException($"0 ControlCode is not Header: expected={HMetadataControlCodes.BookHeader}, value={cc}");

            _header.Metadata.Load(_stream);
            // 读取封面
            cc = ReadNextControlCode(_stream);
            if (cc == HMetadataControlCodes.BookCover)
                throw new InvalidDataException($"2 ControlCode is not Cover: expected={HMetadataControlCodes.BookCover}, value={cc}");

            _coverMetadata.Load(_stream);
            // 跳过封面图像数据
            _stream.Seek(_coverMetadata.ThumbnailLength + _coverMetadata.ImageLength, SeekOrigin.Current);
            // 读取页面
            while (0 != (cc = ReadNextControlCode(_stream)))
            {
                if (cc == HMetadataControlCodes.PageHeader)
                {
                    HMetadataPage page = new HMetadataPage();
                    page.HeaderMetadata.Load(_stream);
                    // 读取页面内容
                    cc = ReadNextControlCode(_stream);
                    if (cc != HMetadataControlCodes.PageContent)
                        throw new InvalidDataException($"Not found page content: pageIndex={_pages.Count}, controlCode={cc}");

                    page.ContentMetadata.Load(_stream);
                    // 跳过图像数据
                    _stream.Seek(page.ContentMetadata.ThumbnailLength + page.ContentMetadata.ImageLength, SeekOrigin.Current);
                    // 添加到集合
                    _pages.Add(page);
                }
                else if (cc == HMetadataControlCodes.VirtualPage)
                {
                    // 忽略虚拟页面
                    HMetadataVirtualPage virtualPage = new HMetadataVirtualPage();
                    virtualPage.Load(_stream);
                }
                else if (cc == HMetadataControlCodes.DeletedPageHeader)
                {
                    // 忽略被删除的页面头
                    HMetadataDeletedPageHeader deletedPage = new HMetadataDeletedPageHeader();
                    deletedPage.Load(_stream);
                }
                else if (cc == HMetadataControlCodes.PageContent)
                {
                    // 忽略被删除的页面内容或没有页头的内容
                    HMetadataPageContent pageContent = new HMetadataPageContent();
                    pageContent.Load(_stream);
                    _stream.Seek(pageContent.ThumbnailLength + pageContent.ImageLength, SeekOrigin.Current);
                }
                else
                    throw new InvalidDataException($"Not support control code: {cc}");
            }// while (0 != (cc = ReadNextControlCode(_stream)))
        }

        public void Create()
        {
            // 存储头


            // 存储封面
            // 存储页面
            // 存储索引
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

            segment.Save(stream, space);
        }

        /// <summary>
        /// 读取下一个控制码
        /// </summary>
        /// <param name="stream">用以读取的Stream</param>
        /// <returns>控制码，0表示已经读到结尾了</returns>
        /// <exception cref="InvalidDataException">没有找到控制码标志<see cref="HMetadataConstant.ControlCodeFlag"/></exception>
        private static byte ReadNextControlCode(Stream stream)
        {
            bool isReadedFlag = false;
            while (stream.Position < stream.Length)
            {
                byte b = (byte)stream.ReadByte();
                if (!isReadedFlag)
                {
                    if (b == HMetadataConstant.ControlCodeFlag)
                        isReadedFlag = true;
                    else
                        throw new InvalidDataException("Not found ControlCodeFlag");
                }
                else
                {
                    if (b != HMetadataConstant.ControlCodeFlag)
                        return b;
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
