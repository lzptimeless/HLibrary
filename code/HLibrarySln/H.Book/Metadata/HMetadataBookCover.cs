﻿using System;
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

        protected override int InitReserveLength { get { return 1024 * 1024; } }

        public int ThumbnailLength { get; set; }
        public const string ThumbnailLengthPropertyName = "ThumbnailLength";

        public int ImageLength { get; set; }
        public const string ImageLengthPropertyName = "ImageLength";

        protected override int GetDataLength()
        {
            // 缩略图长度+图像长度
            return 4 + 4;
        }

        protected override byte[] GetData()
        {
            ExceptionFactory.CheckPropertyRange(ThumbnailLengthPropertyName, ThumbnailLength, 0, int.MaxValue);
            ExceptionFactory.CheckPropertyRange(ImageLengthPropertyName, ImageLength, 0, int.MaxValue);

            int dataLen = GetDataLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;

            // 写入缩略图长度
            writePos += HMetadataHelper.WritePropertyInt(ThumbnailLengthPropertyName, ThumbnailLength, data, writePos);
            // 写入图像长度
            writePos += HMetadataHelper.WritePropertyInt(ImageLengthPropertyName, ImageLength, data, writePos);

            return data;
        }

        protected override void LoadData(byte[] data)
        {
            ExceptionFactory.CheckArgNull("data", data);

            ThumbnailLength = 0;
            ImageLength = 0;

            int readPos = 0;
            // 读取缩略图长度
            int thumbLen;
            readPos += HMetadataHelper.ReadPropertyInt(ThumbnailLengthPropertyName, out thumbLen, data, readPos);
            ThumbnailLength = thumbLen;
            ExceptionFactory.CheckPropertyRange(ThumbnailLengthPropertyName, thumbLen, 0, int.MaxValue);
            // 读取图像长度
            int imgLen;
            readPos += HMetadataHelper.ReadPropertyInt(ThumbnailLengthPropertyName, out imgLen, data, readPos);
            ImageLength = imgLen;
            ExceptionFactory.CheckPropertyRange(ImageLengthPropertyName, imgLen, 0, int.MaxValue);
        }
    }
}
