using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HPageHeaderSetting
    {
        public HPageHeaderFieldSelections Selected { get; set; }
        public string PreArtist { get; set; }
        public string Artist { get; set; }
        public string[] PreCharacters { get; set; }
        public string[] Characters { get; set; }
        public string[] PreTags { get; set; }
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
