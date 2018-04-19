using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public static class HMetadataControlCodes
    {
        /// <summary>
        /// 转义码，这不是一个控制码，程序需要忽略这个字节
        /// The escape code, this is not a control code, program need to ignore this byte
        /// </summary>
        public const byte None = 0x00;
        public const byte BookHeader = 0x01;
        public const byte BookIndex = 0x02;
        public const byte BookCover = 0x11;
        public const byte PageHeader = 0x12;
        public const byte PageContent = 0x19;
        public const byte VirtualPage = 0x13;
        public const byte DeletedPageHeader = 0x10;
    }
}
