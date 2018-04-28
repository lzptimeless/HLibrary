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
            RefreshHeaders();
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
                    RefreshHeaders();
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
            RefreshHeaders();
        }

        public void AddRange(IEnumerable<HMetadataPage> items)
        {
            _items.AddRange(items);
            RefreshHeaders();
        }

        public void Clear()
        {
            if (_items.Count > 0)
            {
                _items.Clear();
                RefreshHeaders();
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
            RefreshHeaders();
        }

        public bool Remove(HMetadataPage item)
        {
            bool result = _items.Remove(item);
            if (result)
                RefreshHeaders();

            return result;
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            RefreshHeaders();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        #endregion

        private void RefreshHeaders()
        {
            var headers = _items.Select(i => new HPageHeader(i.HeaderMetadata));
            Headers = new HPageHeaderCollection(headers);
        }
    }
}
