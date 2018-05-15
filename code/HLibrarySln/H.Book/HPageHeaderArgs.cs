using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HPageHeaderArgs
    {
        public HPageHeaderFieldSelections Selected { get; set; }
        public string Artist { get; set; }
        public string[] Characters { get; set; }
        public string[] Tags { get; set; }
    }

    [Flags]
    public enum HPageHeaderFieldSelections : uint
    {
        Artist = 0x00000001,
        Characters = 0x00000002,
        Tags = 0x00000004,
        All = 0xFFFFFFFF
    }
}
