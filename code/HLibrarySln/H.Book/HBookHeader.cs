using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HBookHeader : IHBookHeader
    {
        public HBookHeader()
        {
            Metadata = new HMetadataBookHeader();
        }

        public HBookHeader(HMetadataBookHeader metadata)
        {
            Metadata = metadata;
        }

        public HMetadataBookHeader Metadata { get; private set; }
        public Guid ID { get { return Metadata.ID; } }
        public byte Version { get { return Metadata.Version; } }
        public string IetfLanguageTag { get { return Metadata.IetfLanguageTag; } }
        public IReadOnlyList<string> Names { get { return Metadata.Names; } }
        public IReadOnlyList<string> Artists { get { return Metadata.Artists; } }
        public IReadOnlyList<string> Groups { get { return Metadata.Groups; } }
        public IReadOnlyList<string> Series { get { return Metadata.Series; } }
        public IReadOnlyList<string> Categories { get { return Metadata.Categories; } }
        public IReadOnlyList<string> Characters { get { return Metadata.Characters; } }
        public IReadOnlyList<string> Tags { get { return Metadata.Tags; } }
    }

    public interface IHBookHeader
    {
        Guid ID { get; }
        byte Version { get; }
        string IetfLanguageTag { get; }
        IReadOnlyList<string> Names { get; }
        IReadOnlyList<string> Artists { get; }
        IReadOnlyList<string> Groups { get; }
        IReadOnlyList<string> Series { get; }
        IReadOnlyList<string> Categories { get; }
        IReadOnlyList<string> Characters { get; }
        IReadOnlyList<string> Tags { get; }
    }
}
