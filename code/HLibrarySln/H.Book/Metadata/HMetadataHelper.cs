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
        /// 向buffer写入list类型的属性值，起始位置会写入1B的list数量
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">被写入的buffer</param>
        /// <param name="startIndex">写入起始位置</param>
        /// <returns></returns>
        public static int WritePropertyList(string propertyName, IList<string> value, byte[] buffer, int startIndex)
        {
            try
            {
                if (value != null)
                    ExceptionFactory.CheckArgCountRange(propertyName, value, 0, byte.MaxValue);

                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);

                int needLen = GetByteLen(value);
                int itemCount = value != null ? value.Count : 0;
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + needLen, int.MaxValue, $"startIndex={startIndex}, needLen={needLen}");

                int writePos = startIndex;
                buffer[writePos] = (byte)itemCount;
                writePos++;

                if (itemCount > 0)
                {
                    foreach (var s in value)
                    {
                        writePos = checked(writePos + StringToBytes(s, buffer, writePos));
                    }
                }

                return writePos - startIndex;
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
        /// <param name="startIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WritePropertyGuid(string propertyName, Guid value, byte[] buffer, int startIndex)
        {
            const int len = 16;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + len, int.MaxValue, $"startIndex={startIndex}, len={len}");

                byte[] valueBuffer = value.ToByteArray();
                ExceptionFactory.CheckBufferLength("valueBuffer", valueBuffer, len);
                Array.Copy(valueBuffer, 0, buffer, startIndex, valueBuffer.Length);
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
        /// <param name="startIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WritePropertyString(string propertyName, string value, byte[] buffer, int startIndex)
        {
            int needLen = GetByteLen(value);
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + needLen, int.MaxValue, $"startIndex={startIndex}, needLen={needLen}");

                StringToBytes(value, buffer, startIndex);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateWritePropertyException(propertyName, null, ex);
            }

            return needLen;
        }

        /// <summary>
        /// 向buffer中写入属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="startIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WritePropertyBool(string propertyName, bool value, byte[] buffer, int startIndex)
        {
            int needLen = 1;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + needLen, int.MaxValue, $"startIndex={startIndex}, needLen={needLen}");

                buffer[startIndex] = (byte)(value ? 1 : 0);
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateWritePropertyException(propertyName, null, ex);
            }

            return needLen;
        }

        /// <summary>
        /// 向buffer中写入属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="buffer">buffer</param>
        /// <param name="startIndex">buffer写入起始位置</param>
        /// <returns></returns>
        public static int WritePropertyInt(string propertyName, int value, byte[] buffer, int startIndex)
        {
            const int len = 4;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + len, int.MaxValue, $"startIndex={startIndex}, len={len}");

                byte[] valueBuffer = BitConverter.GetBytes(value);
                ExceptionFactory.CheckBufferLength("valueBuffer", valueBuffer, len);
                Array.Copy(valueBuffer, 0, buffer, startIndex, valueBuffer.Length);
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
        /// <param name="startIndex">读取起始位置</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyInt(string propertyName, out int value, byte[] buffer, int startIndex)
        {
            const int len = 4;
            value = 0;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckBufferLengthRange("buffer", buffer, startIndex + len, int.MaxValue, $"startIndex={startIndex}, len={len}");

                value = BitConverter.ToInt32(buffer, startIndex);
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
        /// <param name="startIndex">读取起始位置</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyGuid(string propertyName, out Guid value, byte[] buffer, int startIndex)
        {
            const int len = 16;
            value = Guid.Empty;

            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckBufferLengthRange("buffer", buffer, startIndex + len, int.MaxValue, $"startIndex={startIndex}, len={len}");

                byte[] data = new byte[len];
                Array.Copy(buffer, startIndex, data, 0, len);
                value = new Guid(data);
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
        /// <param name="startIndex">读取起始位置</param>
        /// <param name="len">读取字节数</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyString(string propertyName, out string value, byte[] buffer, int startIndex)
        {
            value = null;
            int len = 0;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + 4, int.MaxValue, $"startIndex={startIndex}");

                len = BytesToString(buffer, startIndex, out value);
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
        /// <param name="startIndex">读取起始位置</param>
        /// <param name="len">读取字节数</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyBool(string propertyName, out bool value, byte[] buffer, int startIndex)
        {
            value = false;
            int len = 1;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + len, int.MaxValue, $"startIndex={startIndex}");

                switch (buffer[startIndex])
                {
                    case 1:
                        value = true;
                        break;
                    case 0:
                        value = false;
                        break;
                    default:
                        throw new InvalidDataException($"Not a bool value:{buffer[startIndex]}");
                }
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
        /// <param name="startIndex">读取起始位置</param>
        /// <param name="itemLen">list中每一个string所占的字节数</param>
        /// <returns>读取字节数</returns>
        public static int ReadPropertyList(string propertyName, out string[] value, byte[] buffer, int startIndex)
        {
            value = null;
            int readPos = startIndex;
            try
            {
                ExceptionFactory.CheckArgNull("buffer", buffer);
                ExceptionFactory.CheckArgRange("startIndex", startIndex, 0, buffer.Length - 1);
                ExceptionFactory.CheckArgLengthRange("buffer", buffer, startIndex + 1, int.MaxValue, $"startIndex={startIndex}");

                int count = buffer[readPos];
                readPos++;

                value = new string[count];
                for (int i = 0; i < count; i++)
                {
                    string item;
                    int itemLen = BytesToString(buffer, readPos, out item);
                    readPos += itemLen;
                    value[i] = item;
                }
            }
            catch (Exception ex)
            {
                throw ExceptionFactory.CreateReadPropertyException(propertyName, null, ex);
            }

            return readPos - startIndex;
        }

        /// <summary>
        /// 获取字符串转换为字节后的长度
        /// </summary>
        /// <param name="s">要测量的字符串</param>
        /// <returns>转换为字节后的长度</returns>
        public static int GetByteLen(string s)
        {
            int lenLen = 4;// 长度数据所占字节数
            if (string.IsNullOrEmpty(s)) return lenLen;

            return checked(Encoding.UTF8.GetByteCount(s) + lenLen);
        }

        /// <summary>
        /// 获取字符串数组全部转换为字节后的长度
        /// </summary>
        /// <param name="list">要测量的字符串数组</param>
        /// <returns>全部转换为字节后的长度</returns>
        public static int GetByteLen(IList<string> list)
        {
            int countLen = 1;
            if (list == null || list.Count == 0) return countLen;

            int len = 0;
            foreach (var s in list)
            {
                len = checked(len + GetByteLen(s));
            }

            return checked(len + countLen);
        }

        /// <summary>
        /// 获取字符串UTF8编码数据
        /// </summary>
        /// <param name="s">被编码的字符串</param>
        /// <param name="buffer">存储编码字符串的数组</param>
        /// <param name="index">存储其实位置</param>
        /// <returns>编码字节长度</returns>
        private static int StringToBytes(string s, byte[] buffer, int index)
        {
            int lenLen = 4;// 长度数据所占字节数
            int sLen = string.IsNullOrEmpty(s) ? 0 : Encoding.UTF8.GetByteCount(s);
            byte[] lenBytes = BitConverter.GetBytes(sLen);
            ExceptionFactory.CheckBufferLength("lenBytes", lenBytes, lenLen);
            Array.Copy(lenBytes, 0, buffer, index, lenBytes.Length);

            if (sLen == 0) return lenLen;

            checked
            {
                return Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, index + lenLen) + lenLen;
            }
        }

        /// <summary>
        /// 转换字节为字符串
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="index">起始位置</param>
        /// <returns>结果字符串</returns>
        private static int BytesToString(byte[] buffer, int index, out string s)
        {
            s = string.Empty;
            int lenLen = 4;// 长度数据所占字节数

            int len = BitConverter.ToInt32(buffer, index);
            if (len == 0) return lenLen;

            checked
            {
                s = Encoding.UTF8.GetString(buffer, index + lenLen, len);
                return len + lenLen;
            }
        }
    }
}
