using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataPageCollection : IReadOnlyList<HMetadataPage>
    {
        private List<HMetadataPage> _items = new List<HMetadataPage>();
        private OneManyLock _lock = new OneManyLock();

        public HMetadataPageCollection()
        {
        }

        #region IList<HPage>
        /// <summary>
        /// 获取索引指定的项，返回非null：成功，null：索引不存在
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>返回非null：成功，null：索引不存在</returns>
        public HMetadataPage this[int index]
        {
            get
            {
                HMetadataPage item = null;
                _lock.Enter(false);
                if (index < _items.Count) item = _items[index];
                _lock.Leave();
                return item;
            }
        }

        /// <summary>
        /// 获取指定ID的页面，返回非null：成功，null：失败
        /// </summary>
        /// <param name="id">页面ID</param>
        /// <returns>返回非null：成功，null：失败</returns>
        public HMetadataPage this[Guid id]
        {
            get
            {
                HMetadataPage item = null;
                _lock.Enter(false);
                item = _items.FirstOrDefault(i => i.HeaderMetadata.ID == id);
                _lock.Leave();
                return item;
            }
        }

        public int Count
        {
            get
            {
                int count;
                _lock.Enter(false);
                count = _items.Count;
                _lock.Leave();
                return count;
            }
        }

        /// <summary>
        /// 添加一项，返回false：拥有相同ID的页面已经添加了，true：成功
        /// </summary>
        /// <param name="item">添加的项</param>
        /// <returns>false：拥有相同ID的页面已经添加了，true：成功</returns>
        public bool Add(HMetadataPage item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (item.HeaderMetadata == null) throw new ArgumentException("The property HeaderMetadata can not be null", "item");
            if (item.ContentMetadata == null) throw new ArgumentException("The property ContentMetadata can not be null", "item");

            bool res = false;
            _lock.Enter(true);
            if (!_items.Any(i => i.HeaderMetadata.ID == item.HeaderMetadata.ID))
            {
                _items.Add(item);
                res = true;
            }
            _lock.Leave();
            return res;
        }

        public void Clear()
        {
            _lock.Enter(true);
            _items.Clear();
            _lock.Leave();
        }

        public bool Contains(HMetadataPage item)
        {
            if (item == null) throw new ArgumentNullException("item");

            bool res = false;
            _lock.Enter(false);
            res = _items.Contains(item);
            _lock.Leave();
            return res;
        }

        public bool Contains(Guid id)
        {
            bool res = false;
            _lock.Enter(false);
            res = _items.Any(i => i.HeaderMetadata.ID == id);
            _lock.Leave();
            return res;
        }

        public void CopyTo(HMetadataPage[] array, int arrayIndex)
        {
            _lock.Enter(false);
            _items.CopyTo(array, arrayIndex);
            _lock.Leave();
        }

        public int IndexOf(HMetadataPage item)
        {
            if (item == null) throw new ArgumentNullException("item");

            int index = 0;
            _lock.Enter(false);
            index = _items.IndexOf(item);
            _lock.Leave();
            return index;
        }

        public bool Remove(HMetadataPage item)
        {
            if (item == null) throw new ArgumentNullException("item");

            bool res = false;
            _lock.Enter(true);
            res = _items.Remove(item);
            _lock.Leave();
            return res;
        }

        public bool Remove(Guid id)
        {
            bool res = false;
            _lock.Enter(true);
            int index = -1;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].HeaderMetadata.ID == id)
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0) _items.RemoveAt(index);
            _lock.Leave();
            return res;
        }

        /// <summary>
        /// 移除指定索引的项，返回true：成功，false：索引不存在
        /// </summary>
        /// <param name="index">要移除的项的索引</param>
        /// <returns>true：成功，false：索引不存在</returns>
        public bool RemoveAt(int index)
        {
            bool res = false;
            _lock.Enter(true);
            if (index < _items.Count)
            {
                _items.RemoveAt(index);
                res = true;
            }
            _lock.Leave();
            return res;
        }

        public IEnumerator<HMetadataPage> GetEnumerator()
        {
            IEnumerable<HMetadataPage> clone;
            _lock.Enter(false);
            clone = _items.ToArray();
            _lock.Leave();
            return clone.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<HMetadataPage> clone;
            _lock.Enter(false);
            clone = _items.ToArray();
            _lock.Leave();
            return clone.GetEnumerator();
        }

        public IHPageHeader[] GetPageHeaders()
        {
            IHPageHeader[] headers = null;
            _lock.Enter(false);
            headers = _items.Select(i => new HPageHeader(i.HeaderMetadata)).ToArray();
            _lock.Leave();
            return headers;
        }
        #endregion
    }
}
