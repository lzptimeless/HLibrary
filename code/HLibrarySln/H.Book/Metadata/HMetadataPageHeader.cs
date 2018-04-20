using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataPageHeader : HMetadataBase
    {
        public UInt32 ContentPosition { get; set; }
        public string Artist { get; set; }
        public IList<string> Characters { get; private set; }
        public IList<string> Tags { get; private set; }
    }
}
