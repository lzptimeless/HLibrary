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

        public override int FixedLength { get { return HMetadataConstant.VirtualPageLength; } }

        /// <summary>
        /// Length is 16B
        /// </summary>
        public Guid ID { get; set; }
        public const string IDPropertyName = "ID";

        public Guid BookID { get; set; }
        public string BookIDPropertyName = "BookID";

        public override int GetFieldsLength()
        {
            // 页面ID，书ID
            return 16 + 16;
        }

        protected override byte[] GetFields()
        {
            ExceptionFactory.CheckPropertyEmpty(IDPropertyName, ID);
            ExceptionFactory.CheckPropertyEmpty(BookIDPropertyName, BookID);

            int dataLen = GetFieldsLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;
            // 写入ID
            writePos += HMetadataHelper.WritePropertyGuid(IDPropertyName, ID, data, writePos);
            // 写入书ID
            writePos += HMetadataHelper.WritePropertyGuid(BookIDPropertyName, BookID, data, writePos);

            if (writePos != dataLen)
                throw new WritePropertyException("Unkown", $"Some error occurred in write property: writePos={writePos}, dataLen={dataLen}", null);

            return data;
        }

        protected override void SetFields(byte[] buffer)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);

            int readPos = 0;
            // 读取ID
            Guid id;
            readPos += HMetadataHelper.ReadPropertyGuid(IDPropertyName, out id, buffer, readPos);
            ID = id;
            ExceptionFactory.CheckPropertyEmpty(IDPropertyName, id);
            // 读取书ID
            Guid bookId;
            readPos += HMetadataHelper.ReadPropertyGuid(BookIDPropertyName, out bookId, buffer, readPos);
            BookID = bookId;
            ExceptionFactory.CheckPropertyEmpty(BookIDPropertyName, bookId);
        }

        protected override void OnClone(HMetadataSegment clone)
        {
        }
    }
}
