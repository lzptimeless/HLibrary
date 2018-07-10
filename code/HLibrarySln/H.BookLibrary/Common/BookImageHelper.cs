using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace H.BookLibrary
{
    public static class BookImageHelper
    {
        public static Task<BitmapImage> CreateImageAsync(Stream stream)
        {
            return Task.Run(() =>
            {
                if (stream == null) return null;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            });
        }

        public static BookImageShrinkResult Shrink(Stream imgStream, int maxPixelWidth, int maxPixelHeight)
        {
            if (imgStream == null) throw new ArgumentNullException("imgStream");
            if (maxPixelWidth <= 0) throw new ArgumentOutOfRangeException("maxPixelWidth", maxPixelWidth, "range=[1,int.Max]");
            if (maxPixelHeight <= 0) throw new ArgumentOutOfRangeException("maxPixelHeight", maxPixelHeight, "range=[1,int.Max]");

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imgStream;
            bitmap.CacheOption = BitmapCacheOption.Default;
            bitmap.EndInit();
            bitmap.Freeze();

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            int shrinkWidth = 0;
            int shrinkHeight = 0;
            if (width >= height)
            {
                shrinkWidth = Math.Min(maxPixelWidth, width);
                shrinkHeight = (int)Math.Round((double)height / width * shrinkWidth);
            }
            else
            {
                shrinkHeight = Math.Min(maxPixelHeight, height);
                shrinkWidth = (int)Math.Round((double)width / height * shrinkHeight);
            }

            imgStream.Seek(0, SeekOrigin.Begin);
            // 原始图像大小小于限制值，直接返回原始图像数据
            if (width <= shrinkWidth && height <= shrinkHeight)
                return new BookImageShrinkResult(bitmap, imgStream);

            Output.Print($"Shrink image from {width}x{height} to {shrinkWidth}x{shrinkHeight}");

            BitmapImage thumb = new BitmapImage();
            thumb.BeginInit();
            thumb.StreamSource = imgStream;
            thumb.CacheOption = BitmapCacheOption.Default;
            thumb.DecodePixelWidth = shrinkWidth;
            thumb.DecodePixelHeight = shrinkHeight;
            thumb.EndInit();
            thumb.Freeze();

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumb));
            MemoryStream ms = new MemoryStream();
            try
            {
                encoder.Save(ms);
            }
            catch
            {
                ms.Dispose();
                throw;
            }

            imgStream.Seek(0, SeekOrigin.Begin);
            ms.Seek(0, SeekOrigin.Begin);
            return new BookImageShrinkResult(thumb, ms);
        }
    }

    public class BookImageShrinkResult
    {
        public BookImageShrinkResult(ImageSource imgSrc, Stream s)
        {
            ImageSource = imgSrc;
            Stream = s;
        }

        public ImageSource ImageSource { get; private set; }
        public Stream Stream { get; private set; }
    }
}
