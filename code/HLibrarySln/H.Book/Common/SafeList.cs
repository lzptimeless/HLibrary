using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Book
{
    /// <summary>
    /// 线程安全的列表
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    public class SafeList<T> : IList<T>, IReadOnlyList<T>
    {
        #region fields
        private List<T> _items;
        /// <summary>
        /// 用于缓存<see cref="IEnumerable.GetEnumerator"/>的值
        /// </summary>
        private List<T> _readOnlyItems;
        /// <summary>
        /// 标记<see cref="_readOnlyItems"/>是否需要更新
        /// </summary>
        private bool _isReadOnlyItemsNeedUpdate;
        /// <summary>
        /// 用与<see cref="Interlocked"/>的值，确保线程安全，1：正在操作，0：空闲
        /// </summary>
        private int _lock;
        #endregion

        public SafeList()
        {
            _items = new List<T>();
        }

        public SafeList(IEnumerable<T> items)
        {
            _items = new List<T>(items);
        }

        #region IList<T>

        public T this[int index]
        {
            get
            {
                T item = default(T);
                DoSafe(() =>
                {
                    item = _items[index];
                });
                return item;
            }

            set
            {
                DoSafe(() =>
                {
                    _items[index] = value;
                    _isReadOnlyItemsNeedUpdate = true;
                });
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                DoSafe(() =>
                {
                    count = _items.Count;
                });
                return count;
            }
        }

        public bool IsReadOnly
        {
            get { return ((IList<T>)_items).IsReadOnly; }
        }

        public void Add(T item)
        {
            DoSafe(() =>
            {
                _items.Add(item);
                _isReadOnlyItemsNeedUpdate = true;
            });
        }

        public void AddRange(IEnumerable<T> items)
        {
            DoSafe(() =>
            {
                _items.AddRange(items);
                _isReadOnlyItemsNeedUpdate = true;
            });
        }

        public void Clear()
        {
            DoSafe(() =>
            {
                _items.Clear();
                _isReadOnlyItemsNeedUpdate = true;
            });
        }

        public bool Contains(T item)
        {
            bool result = false;
            DoSafe(() =>
            {
                result = _items.Contains(item);
            });
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            DoSafe(() =>
            {
                _items.CopyTo(array, arrayIndex);
            });
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> e = null;
            DoSafe(() =>
            {
                e = GetReadOnlyItems().GetEnumerator();
            });

            return e;
        }

        public int IndexOf(T item)
        {
            int result = 0;
            DoSafe(() =>
            {
                result = _items.IndexOf(item);
            });
            return result;
        }

        public void Insert(int index, T item)
        {
            DoSafe(() =>
            {
                _items.Insert(index, item);
                _isReadOnlyItemsNeedUpdate = true;
            });
        }

        public bool Remove(T item)
        {
            bool result = false;
            DoSafe(() =>
            {
                result = _items.Remove(item);
                _isReadOnlyItemsNeedUpdate = true;
            });
            return result;
        }

        public void RemoveAt(int index)
        {
            DoSafe(() =>
            {
                _items.RemoveAt(index);
                _isReadOnlyItemsNeedUpdate = true;
            });
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator e = null;
            DoSafe(() =>
            {
                e = GetReadOnlyItems().GetEnumerator();
            });
            return e;
        }

        #endregion

        private List<T> GetReadOnlyItems()
        {
            if (_readOnlyItems == null || _isReadOnlyItemsNeedUpdate)
            {
                _readOnlyItems = new List<T>(_items);
                _isReadOnlyItemsNeedUpdate = false;
            }

            return _readOnlyItems;
        }

        private void DoSafe(Action action)
        {
            // 循环检测空闲状态，由于action执行速度很快，所以这个循环并不会损耗多少资源
            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0) { }

            try
            {
                action.Invoke();
            }
            finally
            {
                // 重置空闲状态
                Volatile.Write(ref _lock, 0);
            }
        }
    }
}
