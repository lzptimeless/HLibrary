using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataPage : HMetadataBase
    {
        public HMetadataPageHeader Header { get; set; }
        public HMetadataPageContent Content { get; set; }
    }
}
