using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataIndex : HMetadataSegment
    {
        public override byte ControlCode { get { return HMetadataControlCodes.BookIndex; } }

        public Guid[] PageIDs { get; set; }
        public const string PageIDsPropertyName = "PageIDs";

        public override int GetFieldsLength()
        {
            // 页面ID+结尾
            checked
            {
                return (PageIDs != null ? PageIDs.Length : 0) * 16 + 16;
            }
        }

        protected override byte[] GetFields()
        {
            ExceptionFactory.CheckPropertyCountRange(PageIDsPropertyName, PageIDs, 0, int.MaxValue);
            if (PageIDs != null && PageIDs.Any(g => g == Guid.Empty))
                throw new InvalidPropertyException(PageIDsPropertyName, "Page ID can not be empty", null);

            int dataLen = GetFieldsLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;

            // 写入页面ID
            if (PageIDs != null)
            {
                for (int i = 0; i < PageIDs.Length; i++)
                    writePos += HMetadataHelper.WritePropertyGuid($"PageIDs[{i}]", PageIDs[i], data, writePos);
            }
            // 写入结尾
            writePos += HMetadataHelper.WritePropertyGuid("end", Guid.Empty, data, writePos);

            if (writePos != dataLen)
                throw new WritePropertyException("Unkown", $"Some error occurred in write property: writePos={writePos}, dataLen={dataLen}", null);

            return data;
        }

        protected override void SetFields(byte[] buffer)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);

            PageIDs = null;
            List<Guid> ids = new List<Guid>();

            int readPos = 0;
            // 读取页ID
            while (true)
            {
                Guid id;
                readPos += HMetadataHelper.ReadPropertyGuid($"PageIDs[{ids.Count}]", out id, buffer, readPos);
                if (id == Guid.Empty) break;

                ids.Add(id);
            }

            PageIDs = ids.ToArray();
        }

        protected override void OnClone(HMetadataSegment clone)
        {
        }

        public void AddPageID(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("id can not be Guid.Empty", "id");

            int newLen = (PageIDs != null ? PageIDs.Length : 0) + 1;
            Guid[] newIDs = new Guid[newLen];
            if (newLen > 1) Array.Copy(PageIDs, 0, newIDs, 0, newLen - 1);

            newIDs[newLen - 1] = id;
            PageIDs = newIDs;
        }
    }
}
