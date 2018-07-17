using H.BookLibrary.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace H.BookLibrary.ViewModels
{
    public class InitViewModel : ViewModelBase
    {
        #region fields
        private ILibraryService _lib;
        #endregion

        #region constructors

        #endregion

        #region properties
        #region ProgressMax
        /// <summary>
        /// Property name of <see cref="ProgressMax"/>
        /// </summary>
        public const string ProgressMaxPropertyName = "ProgressMax";
        private int _progressMax;
        /// <summary>
        /// Get or set <see cref="ProgressMax"/>
        /// </summary>
        public int ProgressMax
        {
            get { return _progressMax; }
            set
            {
                if (_progressMax == value) return;

                _progressMax = value;
                RaisePropertyChanged(ProgressMaxPropertyName);
            }
        }
        #endregion

        #region ProgressValue
        /// <summary>
        /// Property name of <see cref="ProgressValue"/>
        /// </summary>
        public const string ProgressValuePropertyName = "ProgressValue";
        private int _progressValue;
        /// <summary>
        /// Get or set <see cref="ProgressValue"/>
        /// </summary>
        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                if (_progressValue == value) return;

                _progressValue = value;
                RaisePropertyChanged(ProgressValuePropertyName);
            }
        }
        #endregion

        #region Description
        /// <summary>
        /// Property name of <see cref="Description"/>
        /// </summary>
        public const string DescriptionPropertyName = "Description";
        private string _description;
        /// <summary>
        /// Get or set <see cref="Description"/>
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;

                _description = value;
                RaisePropertyChanged(DescriptionPropertyName);
            }
        }
        #endregion
        #endregion

        #region public methods
        public override void Init(FrameworkElement view)
        {
            base.Init(view);
        }

        public async override void ViewLoaded()
        {
            base.ViewLoaded();

            Description = "初始化书库";
            _lib = LibraryService.Instance;
            _lib.InitProgressChanged += _lib_InitProgressChanged;
            await _lib.InitializeAsync();
            Description = "初始化完成";

            // 导航到首页
            HomeViewModel vm = new HomeViewModel();
            vm.ViewManager = ViewManager;
            HomeView view = new HomeView();
            view.DataContext = vm;
            vm.Init(view);

            ViewManager.MainViewSet(view);
        }

        public override void Release()
        {
            if (_lib != null)
            {
                _lib.InitProgressChanged -= _lib_InitProgressChanged;
                _lib = null;
            }

            base.Release();
        }
        #endregion

        #region private methods
        private void _lib_InitProgressChanged(object sender, ProgressEventArgs e)
        {
            if (View.Dispatcher.CheckAccess()) UpdateProgress(e);
            else View.Dispatcher.BeginInvoke((Action)delegate { UpdateProgress(e); });
        }

        private void UpdateProgress(ProgressEventArgs e)
        {
            ProgressMax = e.ProgressMax;
            ProgressValue = e.ProgressValue;
        }
        #endregion
    }
}
