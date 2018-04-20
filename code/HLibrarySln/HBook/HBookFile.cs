using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookFile
    {
        public HMetadataBookHeader Header { get; set; }
        public HMetadataBookCover Cover { get; set; }
        public HMetadataIndex Index { get; set; }
        public IList<HMetadataPage> Pages { get; private set; }
        public IList<HMetadataVirtualPage> VirtualPages { get; private set; }

        public void Load(string path)
        { }

        public void Save()
        { }

        public void SaveAs(string path)
        { }
    }
}
