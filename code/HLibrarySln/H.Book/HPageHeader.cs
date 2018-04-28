using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HPageHeader : IHPageHeader
    {
        public HPageHeader()
        {
            Metadata = new HMetadataPageHeader();
        }

        public HPageHeader(HMetadataPageHeader metadata)
        {
            Metadata = metadata;
        }

        public HMetadataPageHeader Metadata { get; private set; }
        public string Artist { get { return Metadata.Artist; } }
        public IReadOnlyList<string> Charachters { get { return Metadata.Characters; } }
        public IReadOnlyList<string> Tags { get { return Metadata.Tags; } }
    }

    public interface IHPageHeader
    {
        string Artist { get; }
        IReadOnlyList<string> Charachters { get; }
        IReadOnlyList<string> Tags { get; }
    }
}
