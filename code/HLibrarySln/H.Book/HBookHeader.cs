using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HBookHeader : IHBookHeaderReadOnly
    {
        public HBookHeader()
        {
            Names = new List<string>();
            Artists = new List<string>();
            Groups = new List<string>();
            Series = new List<string>();
            Categories = new List<string>();
            Characters = new List<string>();
            Tags = new List<string>();
        }

        public Guid ID { get; set; }
        public byte Version { get; set; }
        public string IetfLanguageTag { get; set; }
        public List<string> Names { get; private set; }
        IReadOnlyList<string> IHBookHeaderReadOnly.Names { get { return Names; } }
        public List<string> Artists { get; private set; }
        IReadOnlyList<string> IHBookHeaderReadOnly.Artists { get { return Artists; } }
        public List<string> Groups { get; private set; }
        IReadOnlyList<string> IHBookHeaderReadOnly.Groups { get { return Groups; } }
        public List<string> Series { get; private set; }
        IReadOnlyList<string> IHBookHeaderReadOnly.Series { get { return Series; } }
        public List<string> Categories { get; private set; }
        IReadOnlyList<string> IHBookHeaderReadOnly.Categories { get { return Categories; } }
        public List<string> Characters { get; private set; }
        IReadOnlyList<string> IHBookHeaderReadOnly.Characters { get { return Characters; } }
        public List<string> Tags { get; private set; }
        IReadOnlyList<string> IHBookHeaderReadOnly.Tags { get { return Tags; } }
    }

    public interface IHBookHeaderReadOnly
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
