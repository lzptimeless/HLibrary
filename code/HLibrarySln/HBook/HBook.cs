using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBook : IHBook
    {
        public HBookHeader Header { get; set; }
        IHBookHeaderReadOnly IHBook.Header { get { return Header; } }
        // IReadOnlyList<IPageHeader> PageHeaders

        // void ReadCover()
        // Stream GetCoverCopy()
        // void ReadCoverThumbnail(Action<Stream> reader)
        // Stream GetCoverThumbnailCopy() 
        // void ReadPage(int index, Action<Stream> reader)
        // Stream GetPageCopy(int index)
        // void ReadThumbnail(int index, Action<Stream> reader)
        // Stream GetThumbnailCopy(int index)
        // void AddPage(int index, PageHeader header, Stream page, Stream thumbnial)
        // void DeletePage(int index)
    }

    public interface IHBook
    {
        IHBookHeaderReadOnly Header { get; }
    }
}
