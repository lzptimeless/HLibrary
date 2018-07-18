using H.Book;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace H.BookLibrary.ViewModels
{
    public class BookMiniModel : BindableBase
    {
        public BookMiniModel(int index, IHBookHeader bookHeader, ImageSource coverThumbnail)
        {
            ID = bookHeader.ID;
            Names = bookHeader.Names;
            Artists = bookHeader.Artists;
            Lang = bookHeader.IetfLanguageTag;
            Groups = bookHeader.Groups;
            Series = bookHeader.Series;
            Categories = bookHeader.Categories;
            Characters = bookHeader.Characters;
            Tags = bookHeader.Tags;
            CoverThumbnail = coverThumbnail;
        }

        #region properties
        #region Index
        /// <summary>
        /// Property name of <see cref="Index"/>
        /// </summary>
        public const string IndexPropertyName = "Index";
        private int _index;
        /// <summary>
        /// Get or set <see cref="Index"/>
        /// </summary>
        public int Index
        {
            get { return this._index; }
            set
            {
                if (this._index == value) return;

                this._index = value;
                this.RaisePropertyChanged(IndexPropertyName);
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

        #region CoverThumbnail
        /// <summary>
        /// Property name of <see cref="CoverThumbnail"/>
        /// </summary>
        public const string CoverThumbnailPropertyName = "CoverThumbnail";
        private ImageSource _coverThumbnail;
        /// <summary>
        /// Get or set <see cref="CoverThumbnail"/>
        /// </summary>
        public ImageSource CoverThumbnail
        {
            get { return _coverThumbnail; }
            set
            {
                if (_coverThumbnail == value) return;

                _coverThumbnail = value;
                RaisePropertyChanged(CoverThumbnailPropertyName);
            }
        }
        #endregion

        #region ID
        /// <summary>
        /// Property name of <see cref="ID"/>
        /// </summary>
        public const string IDPropertyName = "ID";
        private Guid _id;
        /// <summary>
        /// Get or set <see cref="ID"/>
        /// </summary>
        public Guid ID
        {
            get { return _id; }
            set
            {
                if (_id == value) return;

                _id = value;
                RaisePropertyChanged(IDPropertyName);
            }
        }
        #endregion
        #endregion
    }
}
