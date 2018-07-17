using H.Book;
using H.BookLibrary.ViewModels;
using H.BookLibrary.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace H.BookLibrary
{
    public class ShellModel : BindableBase
    {
        #region fields
        private IViewManager _viewManager;
        #endregion

        public ShellModel(IViewManager viewManager)
        {
            _viewManager = viewManager;
        }

        #region properties

        #endregion

        #region commands
        #region MainViewBackCommand

        /// <summary>
        /// MainViewBack command
        /// </summary>
        private DelegateCommand _mainViewBackCommand;
        /// <summary>
        /// Get <see cref="MainViewBackCommand"/>
        /// </summary>
        public ICommand MainViewBackCommand
        {
            get
            {
                if (this._mainViewBackCommand == null)
                {
                    this._mainViewBackCommand = new DelegateCommand(this.MainViewBack, this.CanMainViewBack);
                }

                return this._mainViewBackCommand;
            }
        }

        private void MainViewBack()
        {
            try
            {
                // Do command
                _viewManager.MainViewBack();
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("MainViewBack failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanMainViewBack()
        {
            return true;
        }

        private void RaiseMainViewBackCanExecuteChanged()
        {
            if (this._mainViewBackCommand != null)
            {
                this._mainViewBackCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        #endregion

        #region public methods
        public void ViewLoaded()
        {
            InitViewModel vm = new InitViewModel();
            vm.ViewManager = _viewManager;
            InitView view = new InitView();
            view.DataContext = vm;
            vm.Init(view);

            _viewManager.MainViewSet(view);
        }
        #endregion

        #region private methods

        #endregion
    }
}
