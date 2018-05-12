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

        protected override int InitReserveLength { get { return 10 * 1024; } }

        public int[] PagePositions { get; private set; }
        public const string PagePositionsPropertyName = "PagePositions";

        protected override int GetDataLength()
        {
            // 页面数4B + 页面位置
            int pageCount = PagePositions != null ? PagePositions.Length : 0;
            return 4 + pageCount * 4;
        }

        protected override byte[] GetData()
        {
            ExceptionFactory.CheckPropertyCountRange(PagePositionsPropertyName, PagePositions, 0, int.MaxValue);

            int dataLen = GetDataLength();
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

        protected override void LoadData(byte[] data)
        {
            ExceptionFactory.CheckArgNull("data", data);

            PagePositions = null;

            int readPos = 0;
            // 读取页数
            int pageCount;
            readPos += HMetadataHelper.ReadPropertyInt("PageCount", out pageCount, data, readPos);
            ExceptionFactory.CheckPropertyRange("PageCount", pageCount, 0, int.MaxValue);
            // 读取页位置
            const int posLen = 4;
            int[] pp = new int[pageCount];
            for (int i = 0; i < pageCount; i++)
            {
                int pos = BitConverter.ToInt32(data, readPos);
                pp[i]=pos;
                readPos += posLen;
            }
            PagePositions = pp;
        }
    }
}
