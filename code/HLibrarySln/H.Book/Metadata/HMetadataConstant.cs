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
        /// HBOOK标识码的长度
        /// </summary>
        public const int StartCodeLength = 5;
        /// <summary>
        /// 控制码起始标志
        /// The flag of control code
        /// </summary>
        public const byte CCFlag = 0xFF;
        /// <summary>
        /// 数据校验码0xFE，一些数据字段前会有一个0xFE用来表示数据起始，方便调试
        /// The check code of data
        /// </summary>
        public const byte CCode = 0xFE;
        /// <summary>
        /// 可变长度元数据，用以设置<see cref="HMetadataSegment.FixedLength"/>
        /// </summary>
        public const int VariableLength = -1;
        /// <summary>
        /// 书头固定长度
        /// </summary>
        public const int BookHeaderLength = 4 * 1024;
        /// <summary>
        /// 封面固定长度
        /// </summary>
        public const int BookCoverLength = 1024 * 1024;
        /// <summary>
        /// 封面起始位置
        /// </summary>
        public const int BookCoverPosition = StartCodeLength + BookHeaderLength;
        /// <summary>
        /// 页头固定长度
        /// </summary>
        public const int PageHeaderLength = 1024;
        /// <summary>
        /// 页头列表所占的空间
        /// </summary>
        public const int PageHeaderListLength = 1024 * 1024;
        /// <summary>
        /// 页面内容固定长度
        /// </summary>
        public const int PageContentLength = VariableLength;
        /// <summary>
        /// 页面列表数量位置
        /// </summary>
        public const int PageHeaderListCountPosition = StartCodeLength + BookHeaderLength + BookCoverLength;
        /// <summary>
        /// 第一页头列表起始位置
        /// </summary>
        public const int FirstPageHeaderListPosition = PageHeaderListCountPosition + 1;
        /// <summary>
        /// 第一页头列表结束位置
        /// </summary>
        public const int FirstPageHeaderListEndPosition = FirstPageHeaderListPosition + PageHeaderListLength - 1;
    }
}
