using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtension
    {
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
    }
}
