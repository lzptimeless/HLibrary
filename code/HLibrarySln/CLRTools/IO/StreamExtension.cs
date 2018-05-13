using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtension
    {
        #region fields
        public const int DefaultBufferSize = 4 * 1024;
        #endregion

        public static async Task<int> ReadByteAsync(this Stream stream)
        {
            byte[] buffer = new byte[1];
            int readLen = await stream.ReadAsync(buffer, 0, 1);
            if (readLen == 0)
                return -1;
            else
                return buffer[0];
        }

        public static async Task WriteByteAsync(this Stream stream, byte b)
        {
            byte[] buffer = new byte[] { b };
            await stream.WriteAsync(buffer, 0, 1);
        }

        #region FillAsync
        private static readonly ConcurrentDictionary<byte, byte[]> _fillBuffers = new ConcurrentDictionary<byte, byte[]>();

        private static byte[] GetFillBuffer(byte b)
        {
            byte[] buffer = null;
            if (!_fillBuffers.TryGetValue(b, out buffer))
            {
                buffer = new byte[DefaultBufferSize];
                if (b != 0)
                {
                    for (int i = 0; i < buffer.Length; i++)
                        buffer[i] = b;
                }
                _fillBuffers.TryAdd(b, buffer);
            }
            return buffer;
        }

        public static void Fill(this Stream stream, byte b, int len)
        {
            byte[] buffer = GetFillBuffer(b);
            while (len > 0)
            {
                if (stream.Position >= stream.Length && b == 0)
                {
                    // 如果b==0且文件流已经在末尾，用SetLength效率更高
                    stream.SetLength(checked(len + stream.Length));
                    stream.Seek(0, SeekOrigin.End);
                    break;
                }

                int writeLen = Math.Min(buffer.Length, len);
                stream.Write(buffer, 0, writeLen);
                len = len - writeLen;
            }
        }

        public static async Task FillAsync(this Stream stream, byte b, int len)
        {
            byte[] buffer = GetFillBuffer(b);
            while (len > 0)
            {
                if (stream.Position >= stream.Length && b == 0)
                {
                    // 如果b==0且文件流已经在末尾，用SetLength效率更高
                    stream.SetLength(checked(len + stream.Length));
                    stream.Seek(0, SeekOrigin.End);
                    break;
                }

                int writeLen = Math.Min(buffer.Length, len);
                await stream.WriteAsync(buffer, 0, writeLen);
                len = len - writeLen;
            }
        }
        #endregion
        
    }
}
