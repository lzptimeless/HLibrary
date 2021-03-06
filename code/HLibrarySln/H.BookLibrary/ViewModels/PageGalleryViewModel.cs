﻿using H.Book;
using H.BookLibrary.Views;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace H.BookLibrary.ViewModels
{
    public class PageGalleryViewModel : ViewModelBase
    {
        #region fields
        private ILibraryService _lib;
        private Guid _id;
        private HBookHandle _handle;
        private List<IHPageHeader> _pageHeaders = new List<IHPageHeader>();
        #endregion

        public PageGalleryViewModel(Guid id, int currentPageIndex)
        {
            _lib = LibraryService.Instance;
            _id = id;
            _currentPageIndex = currentPageIndex;

            _pageStretches.AddRange(new[] {
                new SelectionModel<Stretch>("不缩放",Stretch.None),
                new SelectionModel<Stretch>("自适应",Stretch.Uniform),
                new SelectionModel<Stretch>("铺满",Stretch.UniformToFill),
                new SelectionModel<Stretch>("拉伸",Stretch.Fill)
            });
        }

        #region properties
        #region CurrentPage
        /// <summary>
        /// Property name of <see cref="CurrentPage"/>
        /// </summary>
        public const string CurrentPagePropertyName = "CurrentPage";
        private PageMinModel _currentPage;
        /// <summary>
        /// Get or set <see cref="CurrentPage"/>
        /// </summary>
        public PageMinModel CurrentPage
        {
            get { return this._currentPage; }
            set
            {
                if (this._currentPage == value) return;

                this._currentPage = value;
                this.RaisePropertyChanged(CurrentPagePropertyName);

                if (View is PageGalleryView)
                    (View as PageGalleryView).SetPageImage(value != null ? value.Content : null, CurrentPageStretch);
            }
        }
        #endregion

        #region CurrentPageStretch
        /// <summary>
        /// Property name of <see cref="CurrentPageStretch"/>
        /// </summary>
        public const string CurrentPageStretchPropertyName = "CurrentPageStretch";
        private Stretch _currentPageStretch = Stretch.Uniform;
        /// <summary>
        /// Get or set <see cref="CurrentPageStretch"/>
        /// </summary>
        public Stretch CurrentPageStretch
        {
            get { return _currentPageStretch; }
            set
            {
                if (_currentPageStretch == value) return;

                _currentPageStretch = value;
                RaisePropertyChanged(CurrentPageStretchPropertyName);

                if (View is PageGalleryView)
                    (View as PageGalleryView).SetPageImage(CurrentPage != null ? CurrentPage.Content : null, value);
            }
        }
        #endregion

        #region PageStretches
        /// <summary>
        /// Property name of <see cref="PageStretches"/>
        /// </summary>
        public const string PageStretchesPropertyName = "PageStretches";
        private List<SelectionModel<Stretch>> _pageStretches = new List<SelectionModel<Stretch>>();
        /// <summary>
        /// Get or set <see cref="PageStretches"/>
        /// </summary>
        public List<SelectionModel<Stretch>> PageStretches
        {
            get { return _pageStretches; }
            set
            {
                if (_pageStretches == value) return;

                _pageStretches = value;
                RaisePropertyChanged(PageStretchesPropertyName);
            }
        }
        #endregion

        #region CurrentPageIndex
        /// <summary>
        /// Property name of <see cref="CurrentPageIndex"/>
        /// </summary>
        public const string CurrentPageIndexPropertyName = "CurrentPageIndex";
        private int _currentPageIndex;
        /// <summary>
        /// Get or set <see cref="CurrentPageIndex"/>
        /// </summary>
        public int CurrentPageIndex
        {
            get { return _currentPageIndex; }
            set
            {
                if (_currentPageIndex == value) return;

                _currentPageIndex = value;
                RaisePropertyChanged(CurrentPageIndexPropertyName);
            }
        }
        #endregion
        #endregion

        #region commands
        #region PrePageCommand

        /// <summary>
        /// PrePage command
        /// </summary>
        private DelegateCommand _prePageCommand;
        /// <summary>
        /// Get <see cref="PrePageCommand"/>
        /// </summary>
        public ICommand PrePageCommand
        {
            get
            {
                if (this._prePageCommand == null)
                {
                    this._prePageCommand = new DelegateCommand(this.PrePage, this.CanPrePage);
                }

                return this._prePageCommand;
            }
        }

        private async void PrePage()
        {
            try
            {
                // Do command
                if (CurrentPageIndex > 0)
                {
                    CurrentPageIndex = CurrentPageIndex - 1;
                    await LoadCurrentPageAsync();
                }
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("PrePage failed." + Environment.NewLine + ex.ToString());
            }

            RaisePrePageCanExecuteChanged();
            RaiseNextPageCanExecuteChanged();
        }

        private bool CanPrePage()
        {
            return CurrentPageIndex > 0;
        }

        private void RaisePrePageCanExecuteChanged()
        {
            if (this._prePageCommand != null)
            {
                this._prePageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region NextPageCommand

        /// <summary>
        /// NextPage command
        /// </summary>
        private DelegateCommand _nextPageCommand;
        /// <summary>
        /// Get <see cref="NextPageCommand"/>
        /// </summary>
        public ICommand NextPageCommand
        {
            get
            {
                if (this._nextPageCommand == null)
                {
                    this._nextPageCommand = new DelegateCommand(this.NextPage, this.CanNextPage);
                }

                return this._nextPageCommand;
            }
        }

        private async void NextPage()
        {
            try
            {
                // Do command
                if (CurrentPageIndex < _pageHeaders.Count - 1)
                {
                    CurrentPageIndex = CurrentPageIndex + 1;
                    await LoadCurrentPageAsync();
                }
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("NextPage failed." + Environment.NewLine + ex.ToString());
            }

            RaisePrePageCanExecuteChanged();
            RaiseNextPageCanExecuteChanged();
        }

        private bool CanNextPage()
        {
            return CurrentPageIndex < _pageHeaders.Count - 1;
        }

        private void RaiseNextPageCanExecuteChanged()
        {
            if (this._nextPageCommand != null)
            {
                this._nextPageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        #endregion

        #region public methods
        public async override void Init(FrameworkElement view)
        {
            base.Init(view);

            _handle = await _lib.CreateAccessAsync(_id);
            var pagesRes = await _lib.GetPagesAsync(_id, 0, int.MaxValue);
            foreach (var p in pagesRes.Pages)
            {
                _pageHeaders.Add(p.Page);
            }

            await LoadCurrentPageAsync();
            RaisePrePageCanExecuteChanged();
            RaiseNextPageCanExecuteChanged();
        }

        public override void ViewLoaded()
        {
            base.ViewLoaded();
        }

        public override void Release()
        {
            if (_handle != null)
            {
                _lib.ReleaseAccessAsync(_handle);
                _handle = null;
            }

            base.Release();
        }
        #endregion

        #region private methods
        private async Task LoadCurrentPageAsync()
        {
            if (_pageHeaders.Count == 0)
            {
                Output.Print("Page count is 0.");
                return;
            }

            if (_currentPageIndex < 0)
            {
                Output.Print("CurrentPageIndex < 0, reset to 0.");
                CurrentPageIndex = 0;
            }
            if (_currentPageIndex > _pageHeaders.Count - 1)
            {
                Output.Print($"CurrentPageIndex to big({_currentPageIndex}), reset to last index({ _pageHeaders.Count - 1}).");
                CurrentPageIndex = _pageHeaders.Count - 1;
            }

            var ph = _pageHeaders[CurrentPageIndex];
            PageMinModel pcm = new PageMinModel();
            pcm.Index = CurrentPageIndex;
            pcm.Artist = ph.Artist;
            pcm.Characters = ph.Charachters;
            pcm.Tags = ph.Tags;
            pcm.Content = await _lib.GetPageAsync(_handle, ph.ID);
            if (pcm.Content == null) Output.Print($"Page stream is null, index={pcm.Index}");

            CurrentPage = pcm;
        }
        #endregion
    }
}
