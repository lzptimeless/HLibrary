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

        /// <summary>
        /// 向buffer写入list类型的属性值，起始位置会写入1B的list数量
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">被写入的buffer</param>
        /// <param name="bufferStartIndex">写入起始位置</param>
        /// <param name="itemLen">list中每一项所占的固定长度</param>
        /// <returns></returns>
        public static int WriteProperty(string propertyName, IList<string> value, byte[] buffer, int bufferStartIndex, int itemLen)
        {
            try
            {
                if (value != null)
                    ExceptionFactory.CheckArgCountRange(propertyName, value, 0, byte.MaxValue);

                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgRange("itemLen", itemLen, 1, int.MaxValue);

                int itemCount = value != null ? value.Count : 0;
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, itemCount * itemLen + 1, int.MaxValue);

                int writePos = bufferStartIndex;
                buffer[writePos] = (byte)itemCount;
                writePos++;

                if (itemCount > 0)
                {
                    foreach (var s in value)
                    {
                        byte[] itemBuffer = StringToBytes(s, itemLen);
                        Array.Copy(itemBuffer, 0, buffer, writePos, itemBuffer.Length);
                        writePos += itemBuffer.Length;
                    }
                }

                return writePos - bufferStartIndex;
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateWritePropertyException(propertyName, null, ex);
            }
        }

        /// <summary>
        /// 向buffer中写入属性值
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <param name="len">写入数据长度，没有用到部分填入0</param>
        /// <returns></returns>
        public static int WriteProperty(string propertyName, Guid value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 16;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue);

                byte[] valueBuffer = value.ToByteArray();
                ExceptionFactory.CheckBufferLength("valueBuffer", buffer, len);
                Array.Copy(valueBuffer, 0, buffer, bufferStartIndex, valueBuffer.Length);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateWritePropertyException(propertyName, null, ex);
            }

            return len;
        }

        /// <summary>
        /// 向buffer中写入属性值
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <param name="len">写入数据长度，没有用到部分填入0</param>
        /// <returns></returns>
        public static int WriteProperty(string propertyName, string value, byte[] buffer, int bufferStartIndex, int len)
        {
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgRange("len", len, 0, buffer.Length - bufferStartIndex);

                byte[] valueBuffer = StringToBytes(value, len);
                Array.Copy(valueBuffer, 0, buffer, bufferStartIndex, valueBuffer.Length);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateWritePropertyException(propertyName, null, ex);
            }

            return len;
        }

        /// <summary>
        /// 向buffer中写入属性值
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WriteProperty(string propertyName, int value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 4;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue);

                byte[] valueBuffer = BitConverter.GetBytes(value);
                ExceptionFactory.CheckBufferLength("valueBuffer", valueBuffer, len);
                Array.Copy(valueBuffer, 0, buffer, bufferStartIndex, valueBuffer.Length);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateWritePropertyException(propertyName, null, ex);
            }

            return len;
        }

        /// <summary>
        /// 向buffer中写入属性值
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WriteProperty(string propertyName, byte value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 1;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);

                buffer[bufferStartIndex] = value;
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateWritePropertyException(propertyName, null, ex);
            }

            return len;
        }

        public static int ReadProperty(string propertyName, out int value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 4;
            value = 0;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckBufferLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue);

                value = BitConverter.ToInt32(buffer, bufferStartIndex);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return len;
        }

        public static int ReadProperty(string propertyName, out Guid value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 16;
            value = Guid.Empty;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckBufferLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue);

                value = new Guid(buffer);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return len;
        }

        public static int ReadProperty(string propertyName, out string value, byte[] buffer, int bufferStartIndex, int len)
        {
            value = null;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgRange("len", len, 1, buffer.Length - bufferStartIndex);

                value = BytesToString(buffer, bufferStartIndex, len);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return len;
        }

        public static int ReadProperty(string propertyName, out List<string> value, byte[] buffer, int bufferStartIndex, int itemLen)
        {
            value = null;
            int readPos = bufferStartIndex;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgRange("itemLen", itemLen, 1, int.MaxValue);

                int count = buffer[readPos];
                readPos++;

                ExceptionFactory.CheckArgLengthRange("buffer", buffer, readPos + count * itemLen, int.MaxValue);

                value = new List<string>(count);
                for (int i = 0; i < count; i++)
                {
                    string item = BytesToString(buffer, readPos, itemLen);
                    readPos += itemLen;
                    value.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return readPos - bufferStartIndex;
        }

        /// <summary>
        /// 获取字符串UTF8编码数据，buffer长度固定
        /// </summary>
        /// <param name="s">被编码的字符串</param>
        /// <param name="len">生成buffer的长度</param>
        /// <returns></returns>
        public static byte[] StringToBytes(string s, int len)
        {
            ExceptionFactory.CheckArgRange("len", len, 1, int.MaxValue);

            byte[] bytes = new byte[len];
            if (!string.IsNullOrEmpty(s))
                Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);

            return bytes;
        }

        private static string BytesToString(byte[] buffer, int bufferStartIndex, int len)
        {
            string s = Encoding.UTF8.GetString(buffer, bufferStartIndex, len);
            return s;
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
