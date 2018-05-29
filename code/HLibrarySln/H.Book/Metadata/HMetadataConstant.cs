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
        public static byte[] StartCode = new byte[] { 0x48, 0x42, 0x4F, 0x4F, 0x4B };
        /// <summary>
        /// 控制码起始标志
        /// The flag of control code
        /// </summary>
        public const byte CCFlag = 0xFF;
        /// <summary>
        /// 数据校验码
        /// The check code of data
        /// </summary>
        public const byte CCode = 0xFE;
        /// <summary>
        /// 获取默认的保留区大小
        /// </summary>
        /// <param name="controlCode">来自<see cref="HMetadataControlCodes"/></param>
        /// <returns></returns>
        public static int GetDefaultReserveLength(int controlCode)
        {
            switch (controlCode)
            {
                case HMetadataControlCodes.BookHeader:
                    return 10 * 1024;
                case HMetadataControlCodes.BookIndex:
                    return 10 * 1024;
                case HMetadataControlCodes.BookCover:
                    return 1024 * 1024;
                case HMetadataControlCodes.PageHeader:
                case HMetadataControlCodes.DeletedPageHeader:
                    return 2 * 1024;
                default:
                    return 0;
            }
        }
    }
}
