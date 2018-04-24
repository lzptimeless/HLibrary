using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataBookCover : HMetadataSegment
    {
        public override byte ControlCode { get { return HMetadataControlCodes.BookCover; } }
        public int ThumbnailLength { get; set; }
        public int ImageLength { get; set; }

        protected override int GetDataLength()
        {
            // 缩略图长度+图像长度
            return 4 + 4;
        }

        protected override byte[] GetData()
        {
            if (ThumbnailLength < 0)
                throw new ApplicationException($"Invalid ThumbnailLength: expected=[0,{int.MaxValue}], value={ThumbnailLength}");

            if (ImageLength < 0)
                throw new ApplicationException($"Invalid ImageLength: expected=[0,{int.MaxValue}], value={ImageLength}");

            int dataLen = GetDataLength();
            byte[] data = new byte[dataLen];
            byte[] buffer;

            // 写入缩略图长度
            buffer = BitConverter.GetBytes(ThumbnailLength);
            Array.Copy(buffer, 0, data, 0, 4);
            // 写入图像长度
            buffer = BitConverter.GetBytes(ImageLength);
            Array.Copy(buffer, 0, data, 4, 4);

            return data;
        }

        protected override void LoadData(byte[] data)
        {
            int expectedLength = GetDataLength();
            if (data.Length != expectedLength)
                throw new InvalidDataException($"data length error: expected={expectedLength}, value={data.Length}");

            int thumbLen, imgLen;
            // 读取缩略图长度
            thumbLen = BitConverter.ToInt32(data, 0);
            if (thumbLen < 0)
                throw new InvalidDataException($"ThumbnailLength error: expected=[0,{int.MaxValue}], value={thumbLen}");

            // 读取图像长度
            imgLen = BitConverter.ToInt32(data, 4);
            if (imgLen < 0)
                throw new InvalidDataException($"ImageLength error: expected=[0,{int.MaxValue}], value={imgLen}");

            ThumbnailLength = thumbLen;
            ImageLength = imgLen;
        }
    }
}
