using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataVirtualPage : HMetadataSegment
    {
        public override byte ControlCode { get { return HMetadataControlCodes.VirtualPageHeader; } }

        public Guid BookID { get; set; }
        public string BookIDPropertyName = "BookID";

        public int PageIndex { get; set; }
        public string PageIndexPropertyName = "PageIndex";

        public override int GetFieldsLength()
        {
            // 书ID，页面索引
            return 16 + 4;
        }

        protected override byte[] GetFields()
        {
            ExceptionFactory.CheckPropertyEmpty(BookIDPropertyName, BookID);
            ExceptionFactory.CheckPropertyRange(PageIndexPropertyName, PageIndex, 0, int.MaxValue);

            int dataLen = GetFieldsLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;

            // 写入书ID
            writePos += HMetadataHelper.WritePropertyGuid(BookIDPropertyName, BookID, data, writePos);
            // 写入页面索引
            writePos += HMetadataHelper.WritePropertyInt(PageIndexPropertyName, PageIndex, data, writePos);

            return data;
        }

        protected override void SetFields(byte[] buffer)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);

            int readPos = 0;
            // 读取ID
            Guid bookId;
            readPos += HMetadataHelper.ReadPropertyGuid(BookIDPropertyName, out bookId, buffer, readPos);
            BookID = bookId;
            ExceptionFactory.CheckPropertyEmpty(BookIDPropertyName, bookId);
            // 读取页面索引
            int pageIndex;
            readPos += HMetadataHelper.ReadPropertyInt(PageIndexPropertyName, out pageIndex, buffer, readPos);
            PageIndex = pageIndex;
            ExceptionFactory.CheckPropertyRange(PageIndexPropertyName, pageIndex, 0, int.MaxValue);
        }
    }
}
