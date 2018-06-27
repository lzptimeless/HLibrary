using H.Book;
using H.BookLibrary.Views;
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

        public BookDownloadViewModel(string bookid)
        {
            _bookID = bookid;
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

        #region FilePath
        /// <summary>
        /// Property name of <see cref="FilePath"/>
        /// </summary>
        public const string FilePathPropertyName = "FilePath";
        private string _filePath;
        /// <summary>
        /// Get or set <see cref="FilePath"/>
        /// </summary>
        public string FilePath
        {
            get { return this._filePath; }
            set
            {
                if (this._filePath == value) return;

                this._filePath = value;
                this.RaisePropertyChanged(FilePathPropertyName);
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
                FilePath = $"books/hitomi-{_bookID}-{DateTime.Now.ToString("yyyy-MM-dd")}.hb";

                Output.Write += Output_Write;
                _downloader = new HitomiBookDownloader();
                _book = await _downloader.DownloadAsync(_bookID, FilePath);
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
}
