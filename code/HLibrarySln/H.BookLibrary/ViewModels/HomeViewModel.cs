using H.BookLibrary.Views;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace H.BookLibrary.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        #region fields

        #endregion

        #region constructors
        public HomeViewModel()
        {

        }
        #endregion

        #region properties

        #endregion

        #region commands
        #region GoAllBookCommand

        /// <summary>
        /// GoAllBook command
        /// </summary>
        private DelegateCommand _goAllBookCommand;
        /// <summary>
        /// Get <see cref="GoAllBookCommand"/>
        /// </summary>
        public ICommand GoAllBookCommand
        {
            get
            {
                if (this._goAllBookCommand == null)
                {
                    this._goAllBookCommand = new DelegateCommand(this.GoAllBook, this.CanGoAllBook);
                }

                return this._goAllBookCommand;
            }
        }

        private void GoAllBook()
        {
            try
            {
                // Do command
                string bookFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "books");
                AllBookViewModel vm = new AllBookViewModel(bookFolderPath);
                vm.ViewManager = ViewManager;
                AllBookView view = new AllBookView();
                view.DataContext = vm;
                vm.Init(view);

                ViewManager.MainViewForward(view);
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("GoAllBook failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanGoAllBook()
        {
            return true;
        }

        private void RaiseGoAllBookCanExecuteChanged()
        {
            if (this._goAllBookCommand != null)
            {
                this._goAllBookCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region GoDownloadCommand

        /// <summary>
        /// GoDownload command
        /// </summary>
        private DelegateCommand _goDownloadCommand;
        /// <summary>
        /// Get <see cref="GoDownloadCommand"/>
        /// </summary>
        public ICommand GoDownloadCommand
        {
            get
            {
                if (this._goDownloadCommand == null)
                {
                    this._goDownloadCommand = new DelegateCommand(this.GoDownload, this.CanGoDownload);
                }

                return this._goDownloadCommand;
            }
        }

        private void GoDownload()
        {
            try
            {
                // Do command
                BookDownloadViewModel vm = new BookDownloadViewModel();
                vm.ViewManager = ViewManager;
                BookDownloadView view = new BookDownloadView();
                view.DataContext = vm;
                vm.Init(view);

                ViewManager.MainViewForward(view);
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("GoDownload failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanGoDownload()
        {
            return true;
        }

        private void RaiseGoDownloadCanExecuteChanged()
        {
            if (this._goDownloadCommand != null)
            {
                this._goDownloadCommand.RaiseCanExecuteChanged();
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

        #region private methods

        #endregion
    }
}
