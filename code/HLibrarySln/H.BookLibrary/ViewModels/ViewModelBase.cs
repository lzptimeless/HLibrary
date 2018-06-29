using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace H.BookLibrary.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        #region properties
        #region ViewManager
        private IViewManager _viewManager;
        /// <summary>
        /// Get or set <see cref="ViewManager"/>
        /// </summary>
        public IViewManager ViewManager
        {
            get { return _viewManager; }
            set { _viewManager = value; }
        }
        #endregion

        #region View
        private FrameworkElement _view;
        protected FrameworkElement View
        {
            get { return _view; }
        }
        #endregion
        #endregion

        public virtual void Init(FrameworkElement view)
        {
            _view = view;
        }

        public virtual void ViewLoaded()
        {

        }

        public virtual void Release()
        {

        }
    }
}
