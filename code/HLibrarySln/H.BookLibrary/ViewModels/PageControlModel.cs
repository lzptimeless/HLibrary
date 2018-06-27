using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace H.BookLibrary.ViewModels
{
    public class PageControlModel : BindableBase
    {
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
            get { return _index; }
            set
            {
                if (_index == value) return;

                _index = value;
                RaisePropertyChanged(IndexPropertyName);
            }
        }
        #endregion

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
