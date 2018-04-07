using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookPageHeader
    {
        public UInt32 ContentPosition { get; set; }
        public string Artist { get; set; }
        public IList<string> Characters { get; private set; }
        public IList<string> Tags { get; private set; }
    }
}
