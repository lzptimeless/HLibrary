using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public static class HMetadataConstant
    {
        /// <summary>
        /// HBOOK的ASCII码
        /// The ASCII of HBOOK
        /// </summary>
        public static byte[] StartCode = new byte[] { 0x5B, 0x48, 0x42, 0x4F, 0x4F, 0x4B };
        /// <summary>
        /// 控制码起始标志
        /// The flag of control code
        /// </summary>
        public const byte ControlCodeFlag = 0xFF;
        /// <summary>
        /// 控制码总长度：<see cref="ControlCodeFlag"/> + 控制码
        /// </summary>
        public const int ControlCodeTotalLength = 2;
        /// <summary>
        /// 元数据段长度的长度
        /// </summary>
        public static readonly int MetadataSegmentLengthLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(long));
    }
}
