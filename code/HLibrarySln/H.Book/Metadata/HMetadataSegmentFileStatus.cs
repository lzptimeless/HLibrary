using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    /// <summary>
    /// <see cref="HMetadataSegment"/>在文件中的状态
    /// </summary>
    public struct HMetadataSegmentFileStatus
    {
        #region properties
        /// <summary>
        /// 在文件中的位置
        /// </summary>
        public long Position { get; set; }
        public const string PositionPropertyName = "Position";
        /// <summary>
        /// 数据大小 4B 包括除了控制码，本字段本身，保留区大小字段，保留区的所有数据的长度，用以快速定位下一个数据段
        /// </summary>
        public int FieldsLength { get; set; }
        public const string FieldsLengthPropertyName = "FieldsLength";
        /// <summary>
        /// 附加数据长度
        /// </summary>
        public int AppendixLength { get; set; }
        public const string AppendixLengthPropertyName = "AppendixLength";
        /// <summary>
        /// 保留区大小 4B 保留区长度，用以快速定位下一个数据段
        /// </summary>
        public int ReserveLength { get; set; }
        public const string ReserveLengthPropertyName = "ReserveLength";
        #endregion

        #region methods
        /// <summary>
        /// 获取<see cref="HMetadataSegment"/>的头在文件中的大小
        /// </summary>
        /// <returns></returns>
        public int GetHeaderLength()
        {
            // 控制码 + 字段大小字段 + 附加数据长度字段 + 保留区大小字段
            return 2 + 4 + 4 + 4;
        }

        /// <summary>
        /// 获取<see cref="HMetadataSegment"/>在文件中所占空间大小
        /// </summary>
        /// <returns></returns>
        public int GetSpace()
        {
            // 头 + 字段大小 + 附加数据长度 + 保留区长度
            return checked(GetHeaderLength() + FieldsLength + AppendixLength + ReserveLength);
        }
        #endregion
    }
}
