using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HBookHeaderArgs
    {
        public HBookHeaderFieldSelections Selected { get; set; }
        public string IetfLanguageTag { get; set; }
        public string[] Names { get; set; }
        public string[] Artists { get; set; }
        public string[] Groups { get; set; }
        public string[] Series { get; set; }
        public string[] Categories { get; set; }
        public string[] Characters { get; set; }
        public string[] Tags { get; set; }
    }

    [Flags]
    public enum HBookHeaderFieldSelections:uint
    {
        IetfLanguageTag = 0x00000001,
        Names = 0x00000002,
        Artists = 0x00000004,
        Groups = 0x00000008,
        Series = 0x00000010,
        Categories = 0x00000020,
        Characters = 0x00000040,
        Tags = 0x00000080,
        All = 0xFFFFFFFF
    }
}
