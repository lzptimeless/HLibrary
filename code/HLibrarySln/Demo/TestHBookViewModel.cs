using Demo.Converters;
using H.Book;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Demo
{
    public class TestHBookViewModel : BindableBase
    {
        #region fields
        private IHBook _book;
        #endregion

        public TestHBookViewModel()
        {
            _book = CreateHBook();
        }

        #region properties
        #region IsBusy
        /// <summary>
        /// Property name of <see cref="IsBusy"/>
        /// </summary>
        public const string IsBusyPropertyName = "IsBusy";
        private bool _isBusy;
        /// <summary>
        /// Get or set <see cref="IsBusy"/>
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value) return;

                _isBusy = value;
                RaisePropertyChanged(IsBusyPropertyName);
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
        private ObservableCollection<PageViewModel> _pages = new ObservableCollection<PageViewModel>();
        /// <summary>
        /// Get or set <see cref="Pages"/>
        /// </summary>
        public ObservableCollection<PageViewModel> Pages
        {
            get { return _pages; }
        }
        #endregion

        #region InputLang
        /// <summary>
        /// Property name of <see cref="InputLang"/>
        /// </summary>
        public const string InputLangPropertyName = "InputLang";
        private string _inputLang;
        /// <summary>
        /// Get or set <see cref="InputLang"/>
        /// </summary>
        public string InputLang
        {
            get { return _inputLang; }
            set
            {
                if (_inputLang == value) return;

                _inputLang = value;
                RaisePropertyChanged(InputLangPropertyName);
            }
        }
        #endregion

        #region InputName
        /// <summary>
        /// Property name of <see cref="InputName"/>
        /// </summary>
        public const string InputNamePropertyName = "InputName";
        private string _inputName;
        /// <summary>
        /// Get or set <see cref="InputName"/>
        /// </summary>
        public string InputName
        {
            get { return _inputName; }
            set
            {
                if (_inputName == value) return;

                _inputName = value;
                RaisePropertyChanged(InputNamePropertyName);
            }
        }
        #endregion

        #region InputArtist
        /// <summary>
        /// Property name of <see cref="InputArtist"/>
        /// </summary>
        public const string InputArtistPropertyName = "InputArtist";
        private string _inputArtist;
        /// <summary>
        /// Get or set <see cref="InputArtist"/>
        /// </summary>
        public string InputArtist
        {
            get { return _inputArtist; }
            set
            {
                if (_inputArtist == value) return;

                _inputArtist = value;
                RaisePropertyChanged(InputArtistPropertyName);
            }
        }
        #endregion

        #region InputGroup
        /// <summary>
        /// Property name of <see cref="InputGroup"/>
        /// </summary>
        public const string InputGroupPropertyName = "InputGroup";
        private string _inputGroup;
        /// <summary>
        /// Get or set <see cref="InputGroup"/>
        /// </summary>
        public string InputGroup
        {
            get { return _inputGroup; }
            set
            {
                if (_inputGroup == value) return;

                _inputGroup = value;
                RaisePropertyChanged(InputGroupPropertyName);
            }
        }
        #endregion

        #region InputSeries
        /// <summary>
        /// Property name of <see cref="InputSeries"/>
        /// </summary>
        public const string InputSeriesPropertyName = "InputSeries";
        private string _inputSeries;
        /// <summary>
        /// Get or set <see cref="InputSeries"/>
        /// </summary>
        public string InputSeries
        {
            get { return _inputSeries; }
            set
            {
                if (_inputSeries == value) return;

                _inputSeries = value;
                RaisePropertyChanged(InputSeriesPropertyName);
            }
        }
        #endregion

        #region InputCategory
        /// <summary>
        /// Property name of <see cref="InputCategory"/>
        /// </summary>
        public const string InputCategoryPropertyName = "InputCategory";
        private string _inputCategory;
        /// <summary>
        /// Get or set <see cref="InputCategory"/>
        /// </summary>
        public string InputCategory
        {
            get { return _inputCategory; }
            set
            {
                if (_inputCategory == value) return;

                _inputCategory = value;
                RaisePropertyChanged(InputCategoryPropertyName);
            }
        }
        #endregion

        #region InputCharacter
        /// <summary>
        /// Property name of <see cref="InputCharacter"/>
        /// </summary>
        public const string InputCharacterPropertyName = "InputCharacter";
        private string _inputCharacter;
        /// <summary>
        /// Get or set <see cref="InputCharacter"/>
        /// </summary>
        public string InputCharacter
        {
            get { return _inputCharacter; }
            set
            {
                if (_inputCharacter == value) return;

                _inputCharacter = value;
                RaisePropertyChanged(InputCharacterPropertyName);
            }
        }
        #endregion

        #region InputTag
        /// <summary>
        /// Property name of <see cref="InputTag"/>
        /// </summary>
        public const string InputTagPropertyName = "InputTag";
        private string _inputTag;
        /// <summary>
        /// Get or set <see cref="InputTag"/>
        /// </summary>
        public string InputTag
        {
            get { return _inputTag; }
            set
            {
                if (_inputTag == value) return;

                _inputTag = value;
                RaisePropertyChanged(InputTagPropertyName);
            }
        }
        #endregion

        #region InputCoverThumb
        /// <summary>
        /// Property name of <see cref="InputCoverThumb"/>
        /// </summary>
        public const string InputCoverThumbPropertyName = "InputCoverThumb";
        private string _inputCoverThumb;
        /// <summary>
        /// Get or set <see cref="InputCoverThumb"/>
        /// </summary>
        public string InputCoverThumb
        {
            get { return _inputCoverThumb; }
            set
            {
                if (_inputCoverThumb == value) return;

                _inputCoverThumb = value;
                RaisePropertyChanged(InputCoverThumbPropertyName);
            }
        }
        #endregion

        #region InputCover
        /// <summary>
        /// Property name of <see cref="InputCover"/>
        /// </summary>
        public const string InputCoverPropertyName = "InputCover";
        private string _inputCover;
        /// <summary>
        /// Get or set <see cref="InputCover"/>
        /// </summary>
        public string InputCover
        {
            get { return _inputCover; }
            set
            {
                if (_inputCover == value) return;

                _inputCover = value;
                RaisePropertyChanged(InputCoverPropertyName);
            }
        }
        #endregion

        #region InputPageArtist
        /// <summary>
        /// Property name of <see cref="InputPageArtist"/>
        /// </summary>
        public const string InputPageArtistPropertyName = "InputPageArtist";
        private string _inputPageArtist;
        /// <summary>
        /// Get or set <see cref="InputPageArtist"/>
        /// </summary>
        public string InputPageArtist
        {
            get { return _inputPageArtist; }
            set
            {
                if (_inputPageArtist == value) return;

                _inputPageArtist = value;
                RaisePropertyChanged(InputPageArtistPropertyName);
            }
        }
        #endregion

        #region InputPageCharacter
        /// <summary>
        /// Property name of <see cref="InputPageCharacter"/>
        /// </summary>
        public const string InputPageCharacterPropertyName = "InputPageCharacter";
        private string _inputPageCharacter;
        /// <summary>
        /// Get or set <see cref="InputPageCharacter"/>
        /// </summary>
        public string InputPageCharacter
        {
            get { return _inputPageCharacter; }
            set
            {
                if (_inputPageCharacter == value) return;

                _inputPageCharacter = value;
                RaisePropertyChanged(InputPageCharacterPropertyName);
            }
        }
        #endregion

        #region InputPageTag
        /// <summary>
        /// Property name of <see cref="InputPageTag"/>
        /// </summary>
        public const string InputPageTagPropertyName = "InputPageTag";
        private string _inputPageTag;
        /// <summary>
        /// Get or set <see cref="InputPageTag"/>
        /// </summary>
        public string InputPageTag
        {
            get { return _inputPageTag; }
            set
            {
                if (_inputPageTag == value) return;

                _inputPageTag = value;
                RaisePropertyChanged(InputPageTagPropertyName);
            }
        }
        #endregion

        #region InputPageThumb
        /// <summary>
        /// Property name of <see cref="InputPageThumb"/>
        /// </summary>
        public const string InputPageThumbPropertyName = "InputPageThumb";
        private string _inputPageThumb;
        /// <summary>
        /// Get or set <see cref="InputPageThumb"/>
        /// </summary>
        public string InputPageThumb
        {
            get { return _inputPageThumb; }
            set
            {
                if (_inputPageThumb == value) return;

                _inputPageThumb = value;
                RaisePropertyChanged(InputPageThumbPropertyName);
            }
        }
        #endregion

        #region InputPageContent
        /// <summary>
        /// Property name of <see cref="InputPageContent"/>
        /// </summary>
        public const string InputPageContentPropertyName = "InputPageContent";
        private string _inputPageContent;
        /// <summary>
        /// Get or set <see cref="InputPageContent"/>
        /// </summary>
        public string InputPageContent
        {
            get { return _inputPageContent; }
            set
            {
                if (_inputPageContent == value) return;

                _inputPageContent = value;
                RaisePropertyChanged(InputPageContentPropertyName);
            }
        }
        #endregion
        #endregion

        #region commands
        #region SaveHeaderCommand

        /// <summary>
        /// SaveHeader command
        /// </summary>
        private DelegateCommand _saveHeaderCommand;
        /// <summary>
        /// Get <see cref="SaveHeaderCommand"/>
        /// </summary>
        public ICommand SaveHeaderCommand
        {
            get
            {
                if (this._saveHeaderCommand == null)
                {
                    this._saveHeaderCommand = new DelegateCommand(this.SaveHeader, this.CanSaveHeader);
                }

                return this._saveHeaderCommand;
            }
        }

        private async void SaveHeader()
        {
            try
            {
                // Do command
                HBookHeaderSetting setting = new HBookHeaderSetting();
                setting.PreIetfLanguageTag = Lang;
                setting.IetfLanguageTag = InputLang;
                setting.PreNames = Names != null ? Names.ToArray() : null;
                setting.Names = CreateArrayValue(InputName);
                setting.PreArtists = Artists != null ? Artists.ToArray() : null;
                setting.Artists = CreateArrayValue(InputArtist);
                setting.PreGroups = Groups != null ? Groups.ToArray() : null;
                setting.Groups = CreateArrayValue(InputGroup);
                setting.PreSeries = Series != null ? Series.ToArray() : null;
                setting.Series = CreateArrayValue(InputSeries);
                setting.PreCategories = Categories != null ? Categories.ToArray() : null;
                setting.Categories = CreateArrayValue(InputCategory);
                setting.PreCharacters = Characters != null ? Characters.ToArray() : null;
                setting.Characters = CreateArrayValue(InputCharacter);
                setting.PreTags = Tags != null ? Tags.ToArray() : null;
                setting.Tags = CreateArrayValue(InputTag);
                setting.Selected = HBookHeaderFieldSelections.All;

                IsBusy = true;
                await _book.SetHeaderAsync(setting);
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                IsBusy = false;
            }

            Reload();
        }

        private bool CanSaveHeader()
        {
            return true;
        }

        private void RaiseSaveHeaderCanExecuteChanged()
        {
            if (this._saveHeaderCommand != null)
            {
                this._saveHeaderCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region SaveCoverCommand

        /// <summary>
        /// SaveCover command
        /// </summary>
        private DelegateCommand _saveCoverCommand;
        /// <summary>
        /// Get <see cref="SaveCoverCommand"/>
        /// </summary>
        public ICommand SaveCoverCommand
        {
            get
            {
                if (this._saveCoverCommand == null)
                {
                    this._saveCoverCommand = new DelegateCommand(this.SaveCover, this.CanSaveCover);
                }

                return this._saveCoverCommand;
            }
        }

        private async void SaveCover()
        {
            FileStream coverFs = null, thumbFs = null;
            try
            {
                // Do command
                string thumbPath = InputCoverThumb;
                string coverPath = InputCover;
                if (!string.IsNullOrWhiteSpace(thumbPath) && File.Exists(thumbPath))
                    thumbFs = new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, true);

                if (!string.IsNullOrWhiteSpace(coverPath) && File.Exists(coverPath))
                    coverFs = new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, true);

                IsBusy = true;
                await _book.SetCoverAsync(thumbFs, coverFs);
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                IsBusy = false;
                coverFs?.Dispose();
                thumbFs?.Dispose();
            }

            Reload();
        }

        private bool CanSaveCover()
        {
            return true;
        }

        private void RaiseSaveCoverCanExecuteChanged()
        {
            if (this._saveCoverCommand != null)
            {
                this._saveCoverCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region AddPageCommand

        /// <summary>
        /// AddPage command
        /// </summary>
        private DelegateCommand _addPageCommand;
        /// <summary>
        /// Get <see cref="AddPageCommand"/>
        /// </summary>
        public ICommand AddPageCommand
        {
            get
            {
                if (this._addPageCommand == null)
                {
                    this._addPageCommand = new DelegateCommand(this.AddPage, this.CanAddPage);
                }

                return this._addPageCommand;
            }
        }

        private async void AddPage()
        {
            FileStream pageFs = null, thumbFs = null;
            try
            {
                // Do command
                string thumbPath = InputPageThumb;
                string pagePath = InputPageContent;
                if (!string.IsNullOrWhiteSpace(thumbPath) && File.Exists(thumbPath))
                    thumbFs = new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, true);

                if (!string.IsNullOrWhiteSpace(pagePath) && File.Exists(pagePath))
                    pageFs = new FileStream(pagePath, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, true);

                HPageHeaderSetting setting = new HPageHeaderSetting();
                setting.Artist = InputPageArtist;
                setting.Characters = CreateArrayValue(InputPageCharacter);
                setting.Tags = CreateArrayValue(InputPageTag);
                setting.Selected = HPageHeaderFieldSelections.All;
                IsBusy = true;
                await _book.AddPageAsync(setting, thumbFs, pageFs);
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                IsBusy = false;
                pageFs?.Dispose();
                thumbFs?.Dispose();
            }

            Reload();
        }

        private bool CanAddPage()
        {
            return true;
        }

        private void RaiseAddPageCanExecuteChanged()
        {
            if (this._addPageCommand != null)
            {
                this._addPageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        #endregion

        #region public methods
        public async void Reload()
        {
            _book.Dispose();
            _book = CreateHBook();
            await Init();
        }

        public async Task Init()
        {
            IsBusy = true;
            await _book.InitAsync();
            var header = await _book.GetHeaderAsync();
            Lang = header.IetfLanguageTag;
            Names = header.Names;
            Artists = header.Artists;
            Groups = header.Groups;
            Series = header.Series;
            Categories = header.Categories;
            Characters = header.Characters;
            Tags = header.Tags;

            CoverThumb = await CreateImage(_book.GetCoverThumbnailCopyAsync());
            Cover = await CreateImage(_book.GetCoverCopyAsync());

            ArrayToTextConverter att = new ArrayToTextConverter();
            InputLang = Lang;
            InputName = att.Convert(Names, typeof(string), null, CultureInfo.CurrentUICulture) as string;
            InputArtist = att.Convert(Artists, typeof(string), null, CultureInfo.CurrentUICulture) as string;
            InputGroup = att.Convert(Groups, typeof(string), null, CultureInfo.CurrentUICulture) as string;
            InputSeries = att.Convert(Series, typeof(string), null, CultureInfo.CurrentUICulture) as string;
            InputCategory = att.Convert(Categories, typeof(string), null, CultureInfo.CurrentUICulture) as string;
            InputCharacter = att.Convert(Characters, typeof(string), null, CultureInfo.CurrentUICulture) as string;
            InputTag = att.Convert(Tags, typeof(string), null, CultureInfo.CurrentUICulture) as string;

            _pages.Clear();
            var phs = await _book.GetPageHeadersAsync();
            foreach (var ph in phs)
            {
                PageViewModel pvm = new PageViewModel();
                pvm.Artist = ph.Artist;
                pvm.Characters = ph.Charachters;
                pvm.Tags = ph.Tags;
                pvm.Thumb = await CreateImage(_book.GetThumbnailCopyAsync(ph.ID));
                pvm.Content = await CreateImage(_book.GetPageCopyAsync(ph.ID));
                _pages.Add(pvm);
            }
            IsBusy = false;
        }

        private IHBook CreateHBook()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testbook.hb");
            //string path = @"..\..\..\H.BookLibrary\bin\Debug\books\hitomi-955625-2018-07-07-new.hb";
            return new HBook(path, HBookMode.OpenOrCreate, HBookAccess.All, 1);
        }

        private static Task<BitmapImage> CreateImage(Task<Stream> taskStream)
        {
            return Task.Run(async () =>
           {
               var stream = await taskStream;
               if (stream == null) return null;

               BitmapImage bitmap = new BitmapImage();
               bitmap.BeginInit();
               bitmap.StreamSource = stream;
               bitmap.CacheOption = BitmapCacheOption.None;
               bitmap.CreateOptions = BitmapCreateOptions.None;
               bitmap.EndInit();
               bitmap.Freeze();
               return bitmap;
           });
        }

        private static string[] CreateArrayValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
                return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            else
                return null;
        }
        #endregion
    }

    public class PageViewModel : BindableBase
    {
        public PageViewModel()
        {

        }

        #region Artist
        /// <summary>
        /// Property name of <see cref="Artist"/>
        /// </summary>
        public const string ArtistPropertyName = "Artist";
        private string _artist;
        /// <summary>
        /// Get or set <see cref="Artist"/>
        /// </summary>
        public string Artist
        {
            get { return _artist; }
            set
            {
                if (_artist == value) return;

                _artist = value;
                RaisePropertyChanged(ArtistPropertyName);
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

        #region Thumb
        /// <summary>
        /// Property name of <see cref="Thumb"/>
        /// </summary>
        public const string ThumbPropertyName = "Thumb";
        private ImageSource _thumb;
        /// <summary>
        /// Get or set <see cref="Thumb"/>
        /// </summary>
        public ImageSource Thumb
        {
            get { return _thumb; }
            set
            {
                if (_thumb == value) return;

                _thumb = value;
                RaisePropertyChanged(ThumbPropertyName);
            }
        }
        #endregion

        #region Content
        /// <summary>
        /// Property name of <see cref="Content"/>
        /// </summary>
        public const string ContentPropertyName = "Content";
        private ImageSource _content;
        /// <summary>
        /// Get or set <see cref="Content"/>
        /// </summary>
        public ImageSource Content
        {
            get { return _content; }
            set
            {
                if (_content == value) return;

                _content = value;
                RaisePropertyChanged(ContentPropertyName);
            }
        }
        #endregion
    }
}
