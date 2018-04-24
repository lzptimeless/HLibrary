using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataIndex : HMetadataBase
    {
        public UInt16 PageCount { get; set; }
        public IList<UInt32> PagePositions { get; private set; }
    }
}
