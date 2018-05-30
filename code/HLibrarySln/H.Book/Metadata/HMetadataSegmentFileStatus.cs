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
    public class HMetadataSegmentFileStatus : ICloneable
    {
        #region fields
        /// <summary>
        /// Control code len(Control code flag + control code)
        /// </summary>
        private const int CCLen = 2;
        /// <summary>
        /// Check code len
        /// </summary>
        private const int CheckLen = 1;
        /// <summary>
        /// The lenght of data length
        /// </summary>
        private const int LenLen = 4;
        #endregion

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
        /// 附加数据长度集合
        /// </summary>
        public int[] AppendixLengths { get; set; }
        public const string AppendixLengthsPropertyName = "AppendixLengths";
        /// <summary>
        /// 保留区大小 4B 保留区长度，用以快速定位下一个数据段
        /// </summary>
        public int ReserveLength { get; set; }
        public const string ReserveLengthPropertyName = "ReserveLength";
        #endregion

        #region methods
        /// <summary>
        /// 获取<see cref="HMetadataSegment"/>在文件中所占空间大小
        /// </summary>
        /// <returns></returns>
        public int GetSpace()
        {
            checked
            {
                // control code add fields
                int space = CCLen + CheckLen + LenLen + FieldsLength;
                // appendix
                if (AppendixLengths != null)
                {
                    foreach (int appendixLen in AppendixLengths)
                    {
                        space += CheckLen + LenLen + appendixLen;
                    }
                }
                space += CheckLen + LenLen; // Add the end of appendix: 0xFE 0x00 0x00 0x00 0x00
                // reserved
                space += CheckLen + LenLen + ReserveLength;
                return space;
            }
        }

        /// <summary>
        /// 获取附加数据的信息
        /// </summary>
        /// <returns></returns>
        public HMetadataAppendix GetAppendix(int index)
        {
            if (index < 0 || AppendixLengths == null || AppendixLengths.Length <= index)
                return null;

            checked
            {
                // control code add fields
                long position = Position + CCLen + CheckLen + LenLen + FieldsLength;
                // the appendix before index
                for (int i = 0; i < index; i++)
                {
                    position += CheckLen + LenLen + AppendixLengths[i];
                }
                position += CheckLen + LenLen;// Add current appendix checkcode length and lenght of length
                return new HMetadataAppendix(position, AppendixLengths[index]);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
