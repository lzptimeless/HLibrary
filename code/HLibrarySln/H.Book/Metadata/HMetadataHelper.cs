using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    internal static class HMetadataHelper
    {
        /// <summary>
        /// 空字节，用以加快填充空数据
        /// </summary>
        private static readonly byte[] ZeroBuffer = new byte[1024];

        public static int WriteProperty(string propertyName, IList<string> list, byte[] buffer, int bufferStartIndex, int itemLen)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);
            ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 1, buffer.Length - 1);
            ExceptionFactory.CheckArgRange("itemLen", itemLen, 0, int.MaxValue);

            int itemCount = list != null ? list.Count : 0;
            int writePosition = startIndex;
            destination[writePosition] = (byte)itemCount;
            writePosition++;

            if (itemCount > 0)
            {
                byte[] buffer;
                foreach (var s in list)
                {
                    buffer = GetStringBytes(s, itemBytesLen);
                    Array.Copy(buffer, 0, destination, writePosition, buffer.Length);
                    writePosition += buffer.Length;
                }
            }

            return writePosition - startIndex;
        }

        /// <summary>
        /// 向buffer中写入属性值
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <param name="len">写入数据长度，没有用到部分填入0</param>
        /// <returns></returns>
        public static int WriteProperty(string propertyName, Guid value, byte[] buffer, int bufferStartIndex, int len)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);
            ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
            ExceptionFactory.CheckArgRange("len", len, 0, buffer.Length - bufferStartIndex);

            int writePos = bufferStartIndex;
            byte[] valueBuffer = value.ToByteArray();
            ExceptionFactory.CheckBufferLengthRange("valueBuffer", buffer, 1, len);
            Array.Copy(valueBuffer, 0, buffer, writePos, valueBuffer.Length);
            writePos += valueBuffer.Length;

            if (valueBuffer.Length < len)
                writePos += FillZero(buffer, writePos, len - valueBuffer.Length);

            return len;
        }

        /// <summary>
        /// 获取字符串UTF8编码数据，buffer长度固定
        /// </summary>
        /// <param name="s">被编码的字符串</param>
        /// <param name="len">生成buffer的长度</param>
        /// <returns></returns>
        public static byte[] GetStringBytes(string s, int len)
        {
            ExceptionFactory.CheckArgRange("len", len, 1, int.MaxValue);

            byte[] bytes = new byte[len];
            Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);

            return bytes;
        }

        private static int FillZero(byte[] buffer, int startIndex, int len)
        {
            // 由于是内部用函数，为了增加效率，就不检测参数了
            int count = len;
            int writePos = startIndex;
            while (count > 0)
            {
                int currentCopyCount = Math.Min(count, ZeroBuffer.Length);
                Array.Copy(ZeroBuffer, 0, buffer, writePos, currentCopyCount);
                count -= currentCopyCount;
                writePos += currentCopyCount;
            }

            return len;
        }
    }
}
