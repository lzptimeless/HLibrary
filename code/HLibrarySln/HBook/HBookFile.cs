using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookFile
    {
        public HBookHeader Header { get; set; }
        public HBookCover Cover { get; set; }
        public HBookIndex Index { get; set; }
        public IList<HBookPage> Pages { get; private set; }
        public IList<HBookVirtualPage> VirtualPages { get; private set; }
    }
}
