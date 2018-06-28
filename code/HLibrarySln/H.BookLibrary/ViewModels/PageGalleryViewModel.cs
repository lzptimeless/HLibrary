using H.Book;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace H.BookLibrary.ViewModels
{
    public class PageGalleryViewModel : ViewModelBase
    {
        #region fields
        private IHBook _book;
        #endregion

        public PageGalleryViewModel(IHBook book)
        {
            _book = book;
        }

        #region properties
        #region CurrentPage
        /// <summary>
        /// Property name of <see cref="CurrentPage"/>
        /// </summary>
        public const string CurrentPagePropertyName = "CurrentPage";
        private PageControlModel _currentPage;
        /// <summary>
        /// Get or set <see cref="CurrentPage"/>
        /// </summary>
        public PageControlModel CurrentPage
        {
            get { return this._currentPage; }
            set
            {
                if (this._currentPage == value) return;

                this._currentPage = value;
                this.RaisePropertyChanged(CurrentPagePropertyName);
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

        private void PrePage()
        {
            try
            {
                // Do command

            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("PrePage failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanPrePage()
        {
            return true;
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

        private void NextPage()
        {
            try
            {
                // Do command

            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("NextPage failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanNextPage()
        {
            return true;
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
        public override void Init(FrameworkElement view)
        {
            base.Init(view);
        }

        public override void ViewLoaded()
        {
            base.ViewLoaded();
        }

        public override void Release()
        {
            base.Release();
        }
        #endregion
    }
}
