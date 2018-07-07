using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public class BookInfoEventArgs : EventArgs
    {
        public BookInfoEventArgs(string name, int pageCount)
        {
            Name = name;
            PageCount = pageCount;
        }

        #region Name
        private string _name;
        /// <summary>
        /// Get or set <see cref="Name"/>
        /// </summary>
        public string Name
        {
            get { return this._name; }
            private set { this._name = value; }
        }
        #endregion

        #region PageCount
        private int _pageCount;
        /// <summary>
        /// Get or set <see cref="PageCount"/>
        /// </summary>
        public int PageCount
        {
            get { return this._pageCount; }
            private set { this._pageCount = value; }
        }
        #endregion
    }
}
