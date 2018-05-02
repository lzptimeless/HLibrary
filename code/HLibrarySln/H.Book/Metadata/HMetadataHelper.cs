using System;
using System.Collections.Generic;
using System.IO;
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
        /// 空数据,长度1024，以<see cref="HMetadataConstant.ControlCodeFlag"/>填充,用以加快
        /// 填充空白数据的效率
        /// </summary>
        private static readonly byte[] EmptyData;

        static HMetadataHelper()
        {
            // 初始化EmptyData
            byte[] emptyData = new byte[1024];
            for (int i = 0; i < emptyData.Length; i++)
            {
                emptyData[i] = HMetadataConstant.ControlCodeFlag;
            }
            EmptyData = emptyData;
        }

        /// <summary>
        /// 向buffer写入list类型的属性值，起始位置会写入1B的list数量
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">被写入的buffer</param>
        /// <param name="bufferStartIndex">写入起始位置</param>
        /// <param name="itemLen">list中每一项所占的固定长度</param>
        /// <returns></returns>
        public static int WritePropertyList(string propertyName, IList<string> value, byte[] buffer, int bufferStartIndex, int itemLen)
        {
            try
            {
                if (value != null)
                    ExceptionFactory.CheckArgCountRange(propertyName, value, 0, byte.MaxValue);

                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgRange("itemLen", itemLen, 1, int.MaxValue);

                int itemCount = value != null ? value.Count : 0;
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, bufferStartIndex + itemCount * itemLen + 1, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, itemCount={itemCount}, itemLen={itemLen}");

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
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WritePropertyGuid(string propertyName, Guid value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 16;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, len={len}");

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
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <param name="len">写入数据长度，没有用到部分填入0</param>
        /// <returns></returns>
        public static int WritePropertyString(string propertyName, string value, byte[] buffer, int bufferStartIndex, int len)
        {
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgRange("len", len, 1, int.MaxValue);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, len={len}");

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
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="bufferStartIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WritePropertyInt(string propertyName, int value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 4;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, len={len}");

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
        /// 从buffer中读取属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">读取到的属性值</param>
        /// <param name="buffer">存有属性值的buffer</param>
        /// <param name="bufferStartIndex">读取起始位置</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyInt(string propertyName, out int value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 4;
            value = 0;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckBufferLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, len={len}");

                value = BitConverter.ToInt32(buffer, bufferStartIndex);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return len;
        }

        /// <summary>
        /// 从buffer中读取属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">读取到的属性值</param>
        /// <param name="buffer">存有属性值的buffer</param>
        /// <param name="bufferStartIndex">读取起始位置</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyGuid(string propertyName, out Guid value, byte[] buffer, int bufferStartIndex)
        {
            const int len = 16;
            value = Guid.Empty;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckBufferLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, len={len}");

                value = new Guid(buffer);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return len;
        }

        /// <summary>
        /// 从buffer中读取属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">读取到的属性值</param>
        /// <param name="buffer">存有属性值的buffer</param>
        /// <param name="bufferStartIndex">读取起始位置</param>
        /// <param name="len">读取字节数</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyString(string propertyName, out string value, byte[] buffer, int bufferStartIndex, int len)
        {
            value = null;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("bufferStartIndex", bufferStartIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgRange("len", len, 1, int.MaxValue);
                ExceptionFactory.CheckBufferLengthRange("buffer", buffer, bufferStartIndex + len, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, len={len}");

                value = BytesToString(buffer, bufferStartIndex, len);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return len;
        }

        /// <summary>
        /// 从buffer中读取属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">读取到的属性值</param>
        /// <param name="buffer">存有属性值的buffer</param>
        /// <param name="bufferStartIndex">读取起始位置</param>
        /// <param name="itemLen">list中每一个string所占的字节数</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyList(string propertyName, out SafeList<string> value, byte[] buffer, int bufferStartIndex, int itemLen)
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
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, readPos + count * itemLen, int.MaxValue, $"bufferStartIndex={bufferStartIndex}, count={count}, itemLen={itemLen}");

                value = new SafeList<string>();
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
        /// 用<see cref="HMetadataConstant.ControlCodeFlag"/>填充
        /// </summary>
        /// <param name="stream">填充的对象</param>
        /// <param name="len">填充的长度</param>
        public static void FillEmpty(Stream stream, long len)
        {
            while (len > 0)
            {
                int writeLen = (int)Math.Min(EmptyData.Length, len);
                stream.Write(EmptyData, 0, writeLen);
                len = len - writeLen;
            }
        }

        /// <summary>
        /// 获取字符串UTF8编码数据，buffer长度固定
        /// </summary>
        /// <param name="s">被编码的字符串</param>
        /// <param name="len">生成buffer的长度</param>
        /// <returns></returns>
        private static byte[] StringToBytes(string s, int len)
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
