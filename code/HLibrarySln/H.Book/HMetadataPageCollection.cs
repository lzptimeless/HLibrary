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
                if (index < _items.Count) item = _items[index];
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
                item = _items.FirstOrDefault(i => i.HeaderMetadata.ID == id);
                return item;
            }
        }

        public int Count
        {
            get
            {
                int count;
                count = _items.Count;
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

            bool res = false;
            if (!_items.Any(i => i.HeaderMetadata.ID == item.HeaderMetadata.ID))
            {
                _items.Add(item);
                res = true;
            }
            return res;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(HMetadataPage item)
        {
            if (item == null) throw new ArgumentNullException("item");

            bool res = false;
            res = _items.Contains(item);
            return res;
        }

        public bool Contains(Guid id)
        {
            bool res = false;
            res = _items.Any(i => i.HeaderMetadata.ID == id);
            return res;
        }

        public void CopyTo(HMetadataPage[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(HMetadataPage item)
        {
            if (item == null) throw new ArgumentNullException("item");

            int index = 0;
            index = _items.IndexOf(item);
            return index;
        }

        public bool Remove(HMetadataPage item)
        {
            if (item == null) throw new ArgumentNullException("item");

            bool res = false;
            res = _items.Remove(item);
            return res;
        }

        public bool Remove(Guid id)
        {
            bool res = false;
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
            if (index < _items.Count)
            {
                _items.RemoveAt(index);
                res = true;
            }
            return res;
        }

        public IEnumerator<HMetadataPage> GetEnumerator()
        {
            IEnumerable<HMetadataPage> clone;
            clone = _items.ToArray();
            return clone.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<HMetadataPage> clone;
            clone = _items.ToArray();
            return clone.GetEnumerator();
        }

        /// <summary>
        /// 获取所有的页头
        /// </summary>
        /// <returns></returns>
        public IHPageHeader[] GetPageHeaders()
        {
            IHPageHeader[] headers = null;
            headers = _items.Select(i => new HPageHeader(i.HeaderMetadata)).ToArray();
            return headers;
        }

        /// <summary>
        /// 获取可见的页头
        /// </summary>
        /// <returns></returns>
        public IHPageHeader[] GetAvailablePageHeaders()
        {
            IHPageHeader[] headers = null;
            headers = _items.Where(i => !i.HeaderMetadata.IsDeleted).Select(i => new HPageHeader(i.HeaderMetadata)).ToArray();
            return headers;
        }
        #endregion
    }
}
