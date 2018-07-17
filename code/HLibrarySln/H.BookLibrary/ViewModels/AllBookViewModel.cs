using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using Prism.Commands;
using System.Windows.Input;
using H.BookLibrary.Views;
using System.IO;

namespace H.BookLibrary.ViewModels
{
    public class AllBookViewModel : ViewModelBase
    {
        #region fields
        private const int DefaultPageSize = 12;

        private ILibraryService _lib;
        /// <summary>
        /// 这个改变来自ViewModel代码
        /// </summary>
        private bool _isInnerChange;
        #endregion

        public AllBookViewModel(string bookDirectory)
        {
            _lib = LibraryService.Instance;
            _galleryPageSizes.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 50, 60, 70, 80, 90, 100 });
            _galleryPageIndexes.Add(0);
        }

        #region properties
        #region SearchString
        /// <summary>
        /// Property name of <see cref="SearchString"/>
        /// </summary>
        public const string SearchStringPropertyName = "SearchString";
        private string _SearchString;
        /// <summary>
        /// Get or set <see cref="SearchString"/>
        /// </summary>
        public string SearchString
        {
            get { return _SearchString; }
            set
            {
                if (_SearchString == value) return;

                _SearchString = value;
                RaisePropertyChanged(SearchStringPropertyName);
            }
        }
        #endregion

        #region Books
        private ObservableCollection<BookMiniModel> _books = new ObservableCollection<BookMiniModel>();
        /// <summary>
        /// Get or set <see cref="Books"/>
        /// </summary>
        public ObservableCollection<BookMiniModel> Books
        {
            get { return _books; }
        }
        #endregion

        #region GalleryPageIndexes
        /// <summary>
        /// Property name of <see cref="GalleryPageIndexes"/>
        /// </summary>
        public const string GalleryPageIndexesPropertyName = "GalleryPageIndexes";
        private ObservableCollection<int> _galleryPageIndexes = new ObservableCollection<int>();
        /// <summary>
        /// Get or set <see cref="GalleryPageIndexes"/>
        /// </summary>
        public ObservableCollection<int> GalleryPageIndexes
        {
            get { return _galleryPageIndexes; }
            set
            {
                if (_galleryPageIndexes == value) return;

                _galleryPageIndexes = value;
                RaisePropertyChanged(GalleryPageIndexesPropertyName);
            }
        }
        #endregion

        #region CurrentGalleryPageIndex
        /// <summary>
        /// Property name of <see cref="CurrentGalleryPageIndex"/>
        /// </summary>
        public const string CurrentGalleryPageIndexPropertyName = "CurrentGalleryPageIndex";
        private int _currentGalleryPageIndex;
        /// <summary>
        /// Get or set <see cref="CurrentGalleryPageIndex"/>
        /// </summary>
        public int CurrentGalleryPageIndex
        {
            get { return _currentGalleryPageIndex; }
            set
            {
                if (_currentGalleryPageIndex == value) return;

                _currentGalleryPageIndex = value;
                RaisePropertyChanged(CurrentGalleryPageIndexPropertyName);

                if (!_isInnerChange) LoadGalleryPages().NoAwait();
            }
        }
        #endregion

        #region CurrentGalleryPageSize
        /// <summary>
        /// Property name of <see cref="CurrentGalleryPageSize"/>
        /// </summary>
        public const string CurrentGalleryPageSizePropertyName = "CurrentGalleryPageSize";
        private int _currentGalleryPageSize = DefaultPageSize;
        /// <summary>
        /// Get or set <see cref="CurrentGalleryPageSize"/>
        /// </summary>
        public int CurrentGalleryPageSize
        {
            get { return _currentGalleryPageSize; }
            set
            {
                if (_currentGalleryPageSize == value) return;

                int oldValue = _currentGalleryPageSize;
                _currentGalleryPageSize = value;
                RaisePropertyChanged(CurrentGalleryPageSizePropertyName);

                if (!_isInnerChange)
                {
                    // 自动设置页号
                    _isInnerChange = true;
                    int currentOffset = CurrentGalleryPageIndex * oldValue;
                    CurrentGalleryPageIndex = currentOffset / value;
                    _isInnerChange = false;
                    // 刷新内容
                    LoadGalleryPages().NoAwait();
                }
            }
        }
        #endregion

        #region GalleryPageSizes
        /// <summary>
        /// Property name of <see cref="GalleryPageSizes"/>
        /// </summary>
        public const string GalleryPageSizesPropertyName = "GalleryPageSizes";
        private List<int> _galleryPageSizes = new List<int>();
        /// <summary>
        /// Get or set <see cref="GalleryPageSizes"/>
        /// </summary>
        public List<int> GalleryPageSizes
        {
            get { return _galleryPageSizes; }
            set
            {
                if (_galleryPageSizes == value) return;

                _galleryPageSizes = value;
                RaisePropertyChanged(GalleryPageSizesPropertyName);
            }
        }
        #endregion
        #endregion

        #region commands
        #region PreGalleryPageCommand

        /// <summary>
        /// PreGalleryPage command
        /// </summary>
        private DelegateCommand _preGalleryPageCommand;
        /// <summary>
        /// Get <see cref="PreGalleryPageCommand"/>
        /// </summary>
        public ICommand PreGalleryPageCommand
        {
            get
            {
                if (this._preGalleryPageCommand == null)
                {
                    this._preGalleryPageCommand = new DelegateCommand(this.PreGalleryPage, this.CanPreGalleryPage);
                }

                return this._preGalleryPageCommand;
            }
        }

        private async void PreGalleryPage()
        {
            try
            {
                // Do command
                _isInnerChange = true;
                if (CurrentGalleryPageIndex > 0)
                    CurrentGalleryPageIndex = CurrentGalleryPageIndex - 1;

                _isInnerChange = false;
                await LoadGalleryPages();
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("PreGalleryPage failed!" + Environment.NewLine + ex.ToString());
            }

            RaisePreGalleryPageCanExecuteChanged();
            RaiseNextGalleryPageCanExecuteChanged();
        }

        private bool CanPreGalleryPage()
        {
            return CurrentGalleryPageIndex > 0;
        }

        private void RaisePreGalleryPageCanExecuteChanged()
        {
            if (this._preGalleryPageCommand != null)
            {
                this._preGalleryPageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region NextGalleryPageCommand

        /// <summary>
        /// NextGalleryPage command
        /// </summary>
        private DelegateCommand _nextGalleryPageCommand;
        /// <summary>
        /// Get <see cref="NextGalleryPageCommand"/>
        /// </summary>
        public ICommand NextGalleryPageCommand
        {
            get
            {
                if (this._nextGalleryPageCommand == null)
                {
                    this._nextGalleryPageCommand = new DelegateCommand(this.NextGalleryPage, this.CanNextGalleryPage);
                }

                return this._nextGalleryPageCommand;
            }
        }

        private async void NextGalleryPage()
        {
            try
            {
                // Do command
                _isInnerChange = true;
                if (CurrentGalleryPageIndex < GalleryPageIndexes.Count - 1)
                    CurrentGalleryPageIndex = CurrentGalleryPageIndex + 1;

                _isInnerChange = false;
                await LoadGalleryPages();
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("NextGalleryPage failed." + Environment.NewLine + ex.ToString());
            }

            RaisePreGalleryPageCanExecuteChanged();
            RaiseNextGalleryPageCanExecuteChanged();
        }

        private bool CanNextGalleryPage()
        {
            return CurrentGalleryPageIndex < GalleryPageIndexes.Count - 1;
        }

        private void RaiseNextGalleryPageCanExecuteChanged()
        {
            if (this._nextGalleryPageCommand != null)
            {
                this._nextGalleryPageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region GoBookDetailCommand
        /// <summary>
        /// GoBookDetail command
        /// </summary>
        private DelegateCommand<BookMiniModel> _goBookDetailCommand;
        /// <summary>
        /// Get <see cref="GoBookDetailCommand"/>
        /// </summary>
        public ICommand GoBookDetailCommand
        {
            get
            {
                if (this._goBookDetailCommand == null)
                {
                    this._goBookDetailCommand = new DelegateCommand<BookMiniModel>(this.GoBookDetail, this.CanGoBookDetail);
                }

                return this._goBookDetailCommand;
            }
        }

        private void GoBookDetail(BookMiniModel e)
        {
            try
            {
                if (e == null)
                    Output.Print("parameter e is null");
                else
                {
                    // Do command
                    BookDetailViewModel vm = new BookDetailViewModel(e.Path);
                    vm.ViewManager = ViewManager;
                    BookDetailView view = new BookDetailView();
                    view.DataContext = vm;
                    vm.Init(view);

                    ViewManager.MainViewForward(view);
                }
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("GoBookDetail failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanGoBookDetail(BookMiniModel e)
        {
            return e != null;
        }

        private void RaiseGoBookDetailCanExecuteChanged()
        {
            if (this._goBookDetailCommand != null)
            {
                this._goBookDetailCommand.RaiseCanExecuteChanged();
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

            await LoadGalleryPages();
        }

        public override void Release()
        {
            base.Release();
        }
        #endregion

        #region private methods
        private async Task LoadGalleryPages()
        {
            Books.Clear();
            if (View is AllBookView) (View as AllBookView).PagesScrollToTop();

            if (GalleryPageIndexes.Count == 0)
            {
                Output.Print("GalleryPageIndexes is empty");
                return;
            }

            if (CurrentGalleryPageSize <= 0)
                Output.Print("GalleryPageSize invalid: " + CurrentGalleryPageSize);

            int pageSize = CurrentGalleryPageSize > 0 ? CurrentGalleryPageSize : DefaultPageSize;
            int pageIndex = Math.Max(GalleryPageIndexes.First(), Math.Min(GalleryPageIndexes.Last(), CurrentGalleryPageIndex));
            if (pageIndex != CurrentGalleryPageIndex)
                Output.Print("CurrentGalleryPageIndex not in the valid range: " + CurrentGalleryPageIndex);

            var booksRes = await _lib.GetBooksAsync(null, pageIndex * pageSize, pageSize);
            if (booksRes == null || booksRes.Books == null)
                Output.Print("GetBooksAsync result is null.");
            else
            {
                for (int i = 0; i < booksRes.Books.Length; i++)
                {
                    var bookHeader = booksRes.Books[1];
                    var coverThumbnail = await _lib.GetCoverThumbnailAsync(bookHeader.ID);
                    BookMiniModel model = new BookMiniModel(i, bookHeader, coverThumbnail, null);
                    Books.Add(model);

                    if (coverThumbnail == null)
                        Output.Print($"Page thumbnail is null, index={i}");
                }
            }

            GalleryUpdatePageInfo(booksRes);
        }

        private void GalleryUpdatePageInfo(BooksResult booksRes)
        {
            _isInnerChange = true;
            int pageSize = CurrentGalleryPageSize;
            if (pageSize <= 0)
            {
                pageSize = CurrentGalleryPageSize = DefaultPageSize;
                Output.Print($"CurrentGalleryPageSize={pageSize} is invlaid, reset to {DefaultPageSize}");
            }

            int pageCount = (int)Math.Ceiling((double)booksRes.Total / pageSize);
            if (pageCount < GalleryPageIndexes.Count)
            {
                if (CurrentGalleryPageIndex >= pageCount)
                {
                    Output.Print($"CurrentGalleryPageIndex={CurrentGalleryPageIndex} too big, set to last page index {pageCount - 1}");
                    CurrentGalleryPageIndex = pageCount - 1;
                }

                for (int i = GalleryPageIndexes.Count - 1; i >= pageCount; i--)
                    GalleryPageIndexes.RemoveAt(i);
            }
            else if (pageCount > GalleryPageIndexes.Count)
            {
                for (int i = GalleryPageIndexes.Count; i < pageCount; i++)
                    GalleryPageIndexes.Add(i);
            }

            _isInnerChange = false;

            RaisePreGalleryPageCanExecuteChanged();
            RaiseNextGalleryPageCanExecuteChanged();
        }
        #endregion
    }
}
