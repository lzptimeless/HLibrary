using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
