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
        
        public int[] PagePositions { get; set; }
        public const string PagePositionsPropertyName = "PagePositions";

        public override int GetFieldsLength()
        {
            // 页面数4B + 页面位置
            int pageCount = PagePositions != null ? PagePositions.Length : 0;
            return 4 + pageCount * 4;
        }

        protected override byte[] GetFields()
        {
            ExceptionFactory.CheckPropertyCountRange(PagePositionsPropertyName, PagePositions, 0, int.MaxValue);

            int dataLen = GetFieldsLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;
            int pageCount = PagePositions != null ? PagePositions.Length : 0;

            // 写入页数
            writePos += HMetadataHelper.WritePropertyInt("PageCount", pageCount, data, writePos);
            // 写入页位置
            if (pageCount > 0)
            {
                var pps = PagePositions;
                const int posLen = 4;
                for (int i = 0; i < pageCount; i++)
                {
                    byte[] valueBuffer = BitConverter.GetBytes(pps[i]);
                    ExceptionFactory.CheckBufferLength("valueBuffer", valueBuffer, posLen);
                    Array.Copy(valueBuffer, 0, data, writePos, valueBuffer.Length);
                    writePos += valueBuffer.Length;
                }
            }

            return data;
        }

        protected override void SetFields(byte[] buffer)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);

            PagePositions = null;

            int readPos = 0;
            // 读取页数
            int pageCount;
            readPos += HMetadataHelper.ReadPropertyInt("PageCount", out pageCount, buffer, readPos);
            ExceptionFactory.CheckPropertyRange("PageCount", pageCount, 0, int.MaxValue);
            // 读取页位置
            const int posLen = 4;
            int[] pp = new int[pageCount];
            for (int i = 0; i < pageCount; i++)
            {
                int pos = BitConverter.ToInt32(buffer, readPos);
                pp[i]=pos;
                readPos += posLen;
            }
            PagePositions = pp;
        }

        protected override void OnClone(HMetadataSegment clone)
        {
        }
    }
}
