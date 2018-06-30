using H.Book;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using H.BookLibrary.Views;

namespace H.BookLibrary.ViewModels
{
    public class BookDetailViewModel : ViewModelBase
    {
        #region fields
        private const int DefaultPageSize = 10;

        private string _bookPath;
        private IHBook _book;
        private List<IHPageHeader> _pageHeaderInfos = new List<IHPageHeader>();
        /// <summary>
        /// 这个改变来自ViewModel代码
        /// </summary>
        private bool _isInnerChange;
        #endregion

        public BookDetailViewModel(string bookPath)
        {
            _bookPath = bookPath;
            _galleryPageSizes.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 50, 60, 70, 80, 90, 100 });
        }

        #region properties
        #region Names
        /// <summary>
        /// Property name of <see cref="Names"/>
        /// </summary>
        public const string NamesPropertyName = "Names";
        private IReadOnlyList<string> _names;
        /// <summary>
        /// Get or set <see cref="Names"/>
        /// </summary>
        public IReadOnlyList<string> Names
        {
            get { return _names; }
            set
            {
                if (_names == value) return;

                _names = value;
                RaisePropertyChanged(NamesPropertyName);
            }
        }
        #endregion

        #region Artists
        /// <summary>
        /// Property name of <see cref="Artists"/>
        /// </summary>
        public const string ArtistsPropertyName = "Artists";
        private IReadOnlyList<string> _artists;
        /// <summary>
        /// Get or set <see cref="Artists"/>
        /// </summary>
        public IReadOnlyList<string> Artists
        {
            get { return _artists; }
            set
            {
                if (_artists == value) return;

                _artists = value;
                RaisePropertyChanged(ArtistsPropertyName);
            }
        }
        #endregion

        #region Lang
        /// <summary>
        /// Property name of <see cref="Lang"/>
        /// </summary>
        public const string LangPropertyName = "Lang";
        private string _lang;
        /// <summary>
        /// Get or set <see cref="Lang"/>
        /// </summary>
        public string Lang
        {
            get { return _lang; }
            set
            {
                if (_lang == value) return;

                _lang = value;
                RaisePropertyChanged(LangPropertyName);
            }
        }
        #endregion

        #region Groups
        /// <summary>
        /// Property name of <see cref="Groups"/>
        /// </summary>
        public const string GroupsPropertyName = "Groups";
        private IReadOnlyList<string> _groups;
        /// <summary>
        /// Get or set <see cref="Groups"/>
        /// </summary>
        public IReadOnlyList<string> Groups
        {
            get { return _groups; }
            set
            {
                if (_groups == value) return;

                _groups = value;
                RaisePropertyChanged(GroupsPropertyName);
            }
        }
        #endregion

        #region Series
        /// <summary>
        /// Property name of <see cref="Series"/>
        /// </summary>
        public const string SeriesPropertyName = "Series";
        private IReadOnlyList<string> _series;
        /// <summary>
        /// Get or set <see cref="Series"/>
        /// </summary>
        public IReadOnlyList<string> Series
        {
            get { return _series; }
            set
            {
                if (_series == value) return;

                _series = value;
                RaisePropertyChanged(SeriesPropertyName);
            }
        }
        #endregion

        #region Categories
        /// <summary>
        /// Property name of <see cref="Categories"/>
        /// </summary>
        public const string CategoriesPropertyName = "Categories";
        private IReadOnlyList<string> _categories;
        /// <summary>
        /// Get or set <see cref="Categories"/>
        /// </summary>
        public IReadOnlyList<string> Categories
        {
            get { return _categories; }
            set
            {
                if (_categories == value) return;

                _categories = value;
                RaisePropertyChanged(CategoriesPropertyName);
            }
        }
        #endregion

        #region Characters
        /// <summary>
        /// Property name of <see cref="Characters"/>
        /// </summary>
        public const string CharactersPropertyName = "Characters";
        private IReadOnlyList<string> _characters;
        /// <summary>
        /// Get or set <see cref="Characters"/>
        /// </summary>
        public IReadOnlyList<string> Characters
        {
            get { return _characters; }
            set
            {
                if (_characters == value) return;

                _characters = value;
                RaisePropertyChanged(CharactersPropertyName);
            }
        }
        #endregion

        #region Tags
        /// <summary>
        /// Property name of <see cref="Tags"/>
        /// </summary>
        public const string TagsPropertyName = "Tags";
        private IReadOnlyList<string> _tags;
        /// <summary>
        /// Get or set <see cref="Tags"/>
        /// </summary>
        public IReadOnlyList<string> Tags
        {
            get { return _tags; }
            set
            {
                if (_tags == value) return;

                _tags = value;
                RaisePropertyChanged(TagsPropertyName);
            }
        }
        #endregion

        #region CoverThumb
        /// <summary>
        /// Property name of <see cref="CoverThumb"/>
        /// </summary>
        public const string CoverThumbPropertyName = "CoverThumb";
        private ImageSource _coverThumb;
        /// <summary>
        /// Get or set <see cref="CoverThumb"/>
        /// </summary>
        public ImageSource CoverThumb
        {
            get { return _coverThumb; }
            set
            {
                if (_coverThumb == value) return;

                _coverThumb = value;
                RaisePropertyChanged(CoverThumbPropertyName);
            }
        }
        #endregion

        #region Cover
        /// <summary>
        /// Property name of <see cref="Cover"/>
        /// </summary>
        public const string CoverPropertyName = "Cover";
        private ImageSource _cover;
        /// <summary>
        /// Get or set <see cref="Cover"/>
        /// </summary>
        public ImageSource Cover
        {
            get { return _cover; }
            set
            {
                if (_cover == value) return;

                _cover = value;
                RaisePropertyChanged(CoverPropertyName);
            }
        }
        #endregion

        #region Pages
        private ObservableCollection<PageMinModel> _pages = new ObservableCollection<PageMinModel>();
        /// <summary>
        /// Get or set <see cref="Pages"/>
        /// </summary>
        public ObservableCollection<PageMinModel> Pages
        {
            get { return _pages; }
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

                _currentGalleryPageSize = value;
                RaisePropertyChanged(CurrentGalleryPageSizePropertyName);

                if (!_isInnerChange)
                    GalleryUpdatePageInfo().ContinueWith(t => LoadGalleryPages().NoAwait(), TaskContinuationOptions.ExecuteSynchronously).NoAwait();
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

        #region EditCoverCommand

        /// <summary>
        /// EditCover command
        /// </summary>
        private DelegateCommand _editCoverCommand;
        /// <summary>
        /// Get <see cref="EditCoverCommand"/>
        /// </summary>
        public ICommand EditCoverCommand
        {
            get
            {
                if (this._editCoverCommand == null)
                {
                    this._editCoverCommand = new DelegateCommand(this.EditCover, this.CanEditCover);
                }

                return this._editCoverCommand;
            }
        }

        private void EditCover()
        {
            try
            {
                // Do command

            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("EditCover failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanEditCover()
        {
            return true;
        }

        private void RaiseEditCoverCanExecuteChanged()
        {
            if (this._editCoverCommand != null)
            {
                this._editCoverCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region EditBookHeaderCommand

        /// <summary>
        /// EditBookHeader command
        /// </summary>
        private DelegateCommand _editBookHeaderCommand;
        /// <summary>
        /// Get <see cref="EditBookHeaderCommand"/>
        /// </summary>
        public ICommand EditBookHeaderCommand
        {
            get
            {
                if (this._editBookHeaderCommand == null)
                {
                    this._editBookHeaderCommand = new DelegateCommand(this.EditBookHeader, this.CanEditBookHeader);
                }

                return this._editBookHeaderCommand;
            }
        }

        private void EditBookHeader()
        {
            try
            {
                // Do command

            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("EditBookHeader failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanEditBookHeader()
        {
            return true;
        }

        private void RaiseEditBookHeaderCanExecuteChanged()
        {
            if (this._editBookHeaderCommand != null)
            {
                this._editBookHeaderCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region EditPageHeaderCommand

        /// <summary>
        /// EditPageHeader command
        /// </summary>
        private DelegateCommand _editPageHeaderCommand;
        /// <summary>
        /// Get <see cref="EditPageHeaderCommand"/>
        /// </summary>
        public ICommand EditPageHeaderCommand
        {
            get
            {
                if (this._editPageHeaderCommand == null)
                {
                    this._editPageHeaderCommand = new DelegateCommand(this.EditPageHeader, this.CanEditPageHeader);
                }

                return this._editPageHeaderCommand;
            }
        }

        private void EditPageHeader()
        {
            try
            {
                // Do command

            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("EditPageHeader failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanEditPageHeader()
        {
            return true;
        }

        private void RaiseEditPageHeaderCanExecuteChanged()
        {
            if (this._editPageHeaderCommand != null)
            {
                this._editPageHeaderCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region GoPageGalleryCommand
        /// <summary>
        /// GoPageGallery command
        /// </summary>
        private DelegateCommand<PageMinModel> _goPageGalleryCommand;
        /// <summary>
        /// Get <see cref="GoPageGalleryCommand"/>
        /// </summary>
        public ICommand GoPageGalleryCommand
        {
            get
            {
                if (this._goPageGalleryCommand == null)
                {
                    this._goPageGalleryCommand = new DelegateCommand<PageMinModel>(this.GoPageGallery, this.CanGoPageGallery);
                }

                return this._goPageGalleryCommand;
            }
        }

        private void GoPageGallery(PageMinModel e)
        {
            try
            {
                // Do command
                PageGalleryViewModel vm = new PageGalleryViewModel(_book, e.Index);
                vm.ViewManager = ViewManager;
                PageGalleryView view = new PageGalleryView();
                view.DataContext = vm;
                vm.Init(view);

                ViewManager.MainViewForward(view);
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("GoPageGallery failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanGoPageGallery(PageMinModel e)
        {
            return e != null;
        }

        private void RaiseGoPageGalleryCanExecuteChanged()
        {
            if (this._goPageGalleryCommand != null)
            {
                this._goPageGalleryCommand.RaiseCanExecuteChanged();
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

            await LoadBook();
            await UpdateBookHeader();
            await LoadCover();
            await GalleryUpdatePageInfo();
            await LoadGalleryPages();
        }

        public override void Release()
        {
            _book?.Dispose();

            base.Release();
        }
        #endregion

        #region private methods
        private async Task LoadBook()
        {
            _book = new HBook(_bookPath, HBookMode.Open);
            await _book.InitAsync();
        }

        private async Task LoadCover()
        {
            await _book.ReadCoverThumbnailAsync(async s =>
            {
                if (s != null)
                    CoverThumb = await BookImageHelper.CreateImageAsync(s);
                else
                    Output.Print("Cover thumbnial is null.");
            });
            await _book.ReadCoverAsync(async s =>
            {
                if (s != null)
                    Cover = await BookImageHelper.CreateImageAsync(s);
                else
                    Output.Print("Cover is null.");
            });
        }

        private async Task LoadGalleryPages()
        {
            Pages.Clear();
            if (View is BookDetailView) (View as BookDetailView).PagesScrollToTop();

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

            var phs = _pageHeaderInfos;
            if (phs.Count <= pageSize * pageIndex)
            {
                Output.Print("Cached page header count not match with page index");
                return;
            }

            for (int i = pageSize * pageIndex; i < pageSize * (pageIndex + 1) && i < _pageHeaderInfos.Count; i++)
            {
                var ph = _pageHeaderInfos[i];
                var thumbImg = await _book.GetThumbnailCopyAsync(ph.ID);
                PageMinModel pvm = new PageMinModel();
                pvm.Index = i;
                pvm.Artist = ph.Artist;
                pvm.Characters = ph.Charachters;
                pvm.Tags = ph.Tags;
                bool res = await _book.ReadThumbnailAsync(ph.ID, async s =>
                {
                    if (s != null)
                        pvm.Thumb = await BookImageHelper.CreateImageAsync(s);
                    else
                        Output.Print($"Page thumbnail is null, index={i}");
                });
                if (!res) Output.Print($"Read page thumbnail failed, index={i}");

                Pages.Add(pvm);
            }
        }

        private async Task GalleryUpdatePageInfo()
        {
            var phs = await _book.GetPageHeadersAsync();
            _pageHeaderInfos.Clear();
            _pageHeaderInfos.AddRange(phs);

            if (CurrentGalleryPageSize <= 0)
                Output.Print("GalleryPageSize invalid: " + CurrentGalleryPageSize);

            int pageSize = CurrentGalleryPageSize > 0 ? CurrentGalleryPageSize : DefaultPageSize;
            int pageCount = (int)Math.Ceiling((double)phs.Length / pageSize);
            if (GalleryPageIndexes.Count > pageCount)
            {
                _isInnerChange = true;
                if (CurrentGalleryPageIndex >= pageCount)
                    CurrentGalleryPageIndex = pageCount - 1;

                _isInnerChange = false;
                for (int i = GalleryPageIndexes.Count - 1; i >= pageCount; i--)
                    GalleryPageIndexes.RemoveAt(i);
            }
            if (GalleryPageIndexes.Count < pageCount)
            {
                for (int i = GalleryPageIndexes.Count; i < pageCount; i++)
                    GalleryPageIndexes.Add(i);
            }

            RaisePreGalleryPageCanExecuteChanged();
            RaiseNextGalleryPageCanExecuteChanged();
        }

        private async Task UpdateBookHeader()
        {
            var header = await _book.GetHeaderAsync();
            Names = header.Names;
            if (!string.IsNullOrEmpty(header.IetfLanguageTag))
                Lang = LanguageHelper.IETFToZh(header.IetfLanguageTag);

            Artists = header.Artists;
            Groups = header.Groups;
            Series = header.Series;
            Categories = header.Categories;
            Characters = header.Characters;
            Tags = header.Tags;
        }
        #endregion
    }
}
