using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataPageCollection : IList<HMetadataPage>
    {
        private List<HMetadataPage> _items = new List<HMetadataPage>();

        public HMetadataPageCollection()
        {
            Headers = new HPageHeaderCollection(this);
        }

        public HPageHeaderCollection Headers { get; private set; }

        #region IList<HPage>
        public HMetadataPage this[int index]
        {
            get { return _items[index]; }

            set
            {
                if (_items[index] != value)
                {
                    _items[index] = value;
                }
            }
        }

        public HMetadataPage this[Guid id]
        {
            get { return _items.First(i => i.HeaderMetadata.ID.Equals(id)); }

            set
            {
                int index = -1;
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i].HeaderMetadata.ID.Equals(id))
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0 && _items[index] != value)
                {
                    _items[index] = value;
                }
            }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList<HMetadataPage>)_items).IsReadOnly; }
        }

        public void Add(HMetadataPage item)
        {
            _items.Add(item);
        }

        public void AddRange(IEnumerable<HMetadataPage> items)
        {
            _items.AddRange(items);
        }

        public void Clear()
        {
            if (_items.Count > 0)
            {
                _items.Clear();
            }
        }

        public bool Contains(HMetadataPage item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(HMetadataPage[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<HMetadataPage> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(HMetadataPage item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, HMetadataPage item)
        {
            _items.Insert(index, item);
        }

        public bool Remove(HMetadataPage item)
        {
            bool result = _items.Remove(item);
            return result;
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        #endregion
    }
}
