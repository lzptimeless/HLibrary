using H.Book;
using H.BookLibrary.Views;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace H.BookLibrary.ViewModels
{
    public class BookDownloadViewModel : ViewModelBase
    {
        #region fields
        private string _bookID;
        private IHBook _book;
        private HitomiBookDownloader _downloader;
        #endregion

        public BookDownloadViewModel()
        {
            _bookID = "1057991";
        }

        #region properties
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
            get { return this._description; }
            set
            {
                if (this._description == value) return;

                this._description = value;
                this.RaisePropertyChanged(DescriptionPropertyName);
            }
        }
        #endregion

        #region CurrentFilePath
        /// <summary>
        /// Property name of <see cref="CurrentFilePath"/>
        /// </summary>
        public const string CurrentFilePathPropertyName = "CurrentFilePath";
        private string _currentFilePath;
        /// <summary>
        /// Get or set <see cref="CurrentFilePath"/>
        /// </summary>
        public string CurrentFilePath
        {
            get { return this._currentFilePath; }
            set
            {
                if (this._currentFilePath == value) return;

                this._currentFilePath = value;
                this.RaisePropertyChanged(CurrentFilePathPropertyName);
            }
        }
        #endregion

        #region CurrentProgressMax
        /// <summary>
        /// Property name of <see cref="CurrentProgressMax"/>
        /// </summary>
        public const string CurrentProgressMaxPropertyName = "CurrentProgressMax";
        private int _currentProgressMax;
        /// <summary>
        /// Get or set <see cref="CurrentProgressMax"/>
        /// </summary>
        public int CurrentProgressMax
        {
            get { return this._currentProgressMax; }
            set
            {
                if (this._currentProgressMax == value) return;

                this._currentProgressMax = value;
                this.RaisePropertyChanged(CurrentProgressMaxPropertyName);
            }
        }
        #endregion

        #region CurrentProgressValue
        /// <summary>
        /// Property name of <see cref="CurrentProgressValue"/>
        /// </summary>
        public const string CurrentProgressValuePropertyName = "CurrentProgressValue";
        private int _currentProgressValue;
        /// <summary>
        /// Get or set <see cref="CurrentProgressValue"/>
        /// </summary>
        public int CurrentProgressValue
        {
            get { return this._currentProgressValue; }
            set
            {
                if (this._currentProgressValue == value) return;

                this._currentProgressValue = value;
                this.RaisePropertyChanged(CurrentProgressValuePropertyName);
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
            Description = $"开始下载：{_bookID}";
            var view = View as BookDownloadView;
            try
            {
                CurrentFilePath = $"books/hitomi-{_bookID}-{DateTime.Now.ToString("yyyy-MM-dd")}.hb";

                Output.Write += Output_Write;
                _downloader = new HitomiBookDownloader();
                _downloader.ProgressChanged += _downloader_ProgressChanged;
                _book = await _downloader.DownloadAsync(_bookID, CurrentFilePath);
                _downloader.ProgressChanged -= _downloader_ProgressChanged;
            }
            catch (Exception ex)
            {
                Description = "下载失败";

                if (view != null) view.Print("下载失败." + Environment.NewLine + ex.ToString());
                return;
            }

            Description = "下载成功";
            if (view != null) view.Print("下载成功.");
        }

        public override void Release()
        {
            Output.Write -= Output_Write;
            base.Release();
        }
        #endregion

        #region private methods
        private void _downloader_ProgressChanged(object sender, DownloadProgressEventArgs e)
        {
            Action action = () => {
                CurrentProgressMax = e.ProgressMax;
                CurrentProgressValue = e.ProgressValue;
            };

            if (View.Dispatcher.CheckAccess()) action.Invoke();
            else View.Dispatcher.InvokeAsync(action);
        }

        private async void Output_Write(object sender, WriteEventArgs e)
        {
            var view = View as BookDownloadView;
            if (view != null && view.IsLoaded)
            {
                if (view.Dispatcher.CheckAccess()) view.Print(e.Message);
                else await view.Dispatcher.InvokeAsync(() => view.Print(e.Message));
            }
        }
        #endregion
    }

    public class BookDownloadItemModel : BindableBase
    {
        public BookDownloadItemModel()
        { }


    }
}
