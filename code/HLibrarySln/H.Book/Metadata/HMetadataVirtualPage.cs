using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataVirtualPage : HMetadataBase
    {
        public Guid BookID { get; set; }
        public UInt16 PageIndex { get; set; }
    }
}
