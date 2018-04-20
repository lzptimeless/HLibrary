using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public abstract class HMetadataSegment
    {
        /// <summary>
        /// 数据段控制码，参见<see cref="HMetadataControlCodes"/>
        /// </summary>
        public abstract byte ControlCode { get; }
        /// <summary>
        /// 数据段在整个<see cref="HBook"/>中的起始位置
        /// </summary>
        public long Position { get; set; }
        /// <summary>
        /// 数据段总大小，包括保留区，包括控制码，用以快速定位下一个数据段
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="stream">用以保存数据的<see cref="Stream"/></param>
        /// <param name="space">stream中可用以保存数据的空间大小，用剩的空间会以<see cref="HMetadataConstant.ControlCodeFlag"/>填充</param>
        public void Save(Stream stream, long space)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            byte[] buffer;
            byte[] data = GetBytes();
            if (data == null)
                throw new ApplicationException("GetBytes return null");

            long totalLength = data.LongLength + HMetadataConstant.ControlCodeTotalLength + HMetadataConstant.MetadataSegmentLengthLength;
            if (totalLength > space)
                throw new ArgumentOutOfRangeException("space", $"The space is not enough to save data: need-len={totalLength}");

            // 写入控制码
            stream.WriteByte(HMetadataConstant.ControlCodeFlag);
            stream.WriteByte(ControlCode);
            // 写入数据段大小
            buffer = BitConverter.GetBytes(space);
            stream.Write(buffer, 0, buffer.Length);
            // 写入数据
            stream.Write(data, 0, data.Length);
            // 填充剩余空间
            
        }

        protected abstract byte[] GetBytes();
    }
}
