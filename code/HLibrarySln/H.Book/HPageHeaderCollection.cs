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
        private List<IHPageHeader> _items;

        public HPageHeaderCollection(IEnumerable<IHPageHeader> items)
        {
            _items = new List<IHPageHeader>(items);
        }

        public IHPageHeader this[int index]
        {
            get { return _items[index]; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public IEnumerator<IHPageHeader> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
