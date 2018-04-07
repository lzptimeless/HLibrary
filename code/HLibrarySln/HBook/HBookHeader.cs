using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookHeader
    {
        public byte Version { get; set; }
        /// <summary>
        /// Length is 128B
        /// </summary>
        public Guid ID { get; set; }
        public UInt32 CoverPosition { get; set; }
        public UInt32 IndexPosition { get; set; }
        /// <summary>
        /// Length less than 32B
        /// </summary>
        public string IetfLanguageTag { get; set; }
        /// <summary>
        /// Each name length less that 128B
        /// </summary>
        public IList<string> Names { get; private set; }
        /// <summary>
        /// Each artist length less that 128B
        /// </summary>
        public IList<string> Artists { get; private set; }
        /// <summary>
        /// Each group length less that 128B
        /// </summary>
        public IList<string> Groups { get; private set; }
        /// <summary>
        /// Each series length less that 128B
        /// </summary>
        public IList<string> Series { get; private set; }
        /// <summary>
        /// Each category length less that 128B
        /// </summary>
        public IList<string> Categories { get; private set; }
        /// <summary>
        /// Each character length less that 128B
        /// </summary>
        public IList<string> Characters { get; private set; }
        /// <summary>
        /// Each rag length less that 64B
        /// </summary>
        public IList<string> Tags { get; private set; }
    }
}
