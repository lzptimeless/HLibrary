using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace H.BookLibrary
{
    public class CoverEventArgs : EventArgs
    {
        public CoverEventArgs(ImageSource thumbnail, ImageSource content)
        {
            Thumbnail = thumbnail;
            Content = content;
        }

        public ImageSource Thumbnail { get; private set; }
        public ImageSource Content { get; private set; }
    }
}
