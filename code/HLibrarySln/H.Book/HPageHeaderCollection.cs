using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HPageHeaderCollection : IReadOnlyList<IHPageHeader>
    {
        private HMetadataPageCollection _pages;

        public HPageHeaderCollection(HMetadataPageCollection pages)
        {
            _pages = pages;
        }

        public IHPageHeader this[int index]
        {
            get { return CreateReadOnlyHeader(_pages[index]); }
        }

        public IHPageHeader this[Guid id]
        {
            get { return CreateReadOnlyHeader(_pages[id]); }
        }

        public int Count
        {
            get { return _pages.Count; }
        }

        public IEnumerator<IHPageHeader> GetEnumerator()
        {
            foreach (var p in _pages)
            {
                yield return CreateReadOnlyHeader(p);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var p in _pages)
            {
                yield return CreateReadOnlyHeader(p);
            }
        }

        private IHPageHeader CreateReadOnlyHeader(HMetadataPage page)
        {
            return new HPageHeader(page.HeaderMetadata);
        }
    }
}
