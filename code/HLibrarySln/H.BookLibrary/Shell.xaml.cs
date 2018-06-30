using H.Book;
using H.BookLibrary.ViewModels;
using H.BookLibrary.Views;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace H.BookLibrary
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window, IViewManager
    {
        public Shell()
        {
            InitializeComponent();

            ShellModel model = new ShellModel(this);
            DataContext = model;

            Loaded += Shell_Loaded;
            KeyUp += Shell_KeyUp;
            MouseUp += Shell_MouseUp;
        }

        #region properties
        #endregion

        #region commands
        #region BackCommand
        /// <summary>
        /// Back command
        /// </summary>
        private DelegateCommand<int?> _backCommand;
        /// <summary>
        /// Get <see cref="BackCommand"/>
        /// </summary>
        public ICommand BackCommand
        {
            get
            {
                if (this._backCommand == null)
                {
                    this._backCommand = new DelegateCommand<int?>(this.Back, this.CanBack);
                }

                return this._backCommand;
            }
        }

        private void Back(int? e)
        {
            try
            {
                // Do command
                if (e.HasValue) MainViewBack(e.Value);
                else Output.Print("Back index is null.");
            }
            catch (Exception ex)
            {
                // Do exception work, print log.
                Output.Print("Back failed." + Environment.NewLine + ex.ToString());
            }
        }

        private bool CanBack(int? e)
        {
            return e.HasValue;
        }

        private void RaiseBackCanExecuteChanged()
        {
            if (this._backCommand != null)
            {
                this._backCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        #endregion

        public void MainViewSet(IView view)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (!(view is UIElement)) throw new ArgumentException("view must be UIElement", "view");

            // 移除所有页面
            PageHost.Children.Clear();
            // 添加新页面
            PageHost.Children.Add(view as UIElement);

            UpdateBreadcrumb();
        }

        public void MainViewForward(IView view)
        {
            if (view == null) throw new ArgumentNullException("view");

            // 隐藏当前页面
            if (PageHost.Children.Count > 0)
            {
                var currentView = PageHost.Children[PageHost.Children.Count - 1];
                currentView.Visibility = Visibility.Collapsed;
            }

            PageHost.Children.Add(view as UIElement);

            UpdateBreadcrumb();
        }

        public void MainViewBack()
        {
            MainViewBack(Math.Max(0, PageHost.Children.Count - 2));
        }

        public void MainViewBack(int index)
        {
            if (PageHost.Children.Count == 0) throw new ApplicationException("Not exist any MainView");
            if (PageHost.Children.Count <= index || index < 0)
                throw new ArgumentOutOfRangeException("index", $"available range=[0,{PageHost.Children.Count - 1}]");

            // 如果请求后退目的索引等于当前页面则不处理
            if (PageHost.Children.Count - 1 == index) return;

            // 移除页面
            for (int i = PageHost.Children.Count - 1; i > index; i--)
                PageHost.Children.RemoveAt(i);

            // 显示页面
            var tagView = PageHost.Children[index];
            tagView.Visibility = Visibility.Visible;

            UpdateBreadcrumb();
        }

        private void UpdateBreadcrumb()
        {
            BreadcrumbTextBlock.Inlines.Clear();
            for (int i = 0; i < PageHost.Children.Count; i++)
            {
                var view = PageHost.Children[i] as IView;
                if (view == null) throw new ApplicationException($"The view({i}), is not a IView");

                Hyperlink hl = new Hyperlink(new Run(view.Title));
                hl.CommandParameter = i;
                hl.Command = BackCommand;

                BreadcrumbTextBlock.Inlines.Add(hl);

                // 添加间隔符号'>'
                if (i < PageHost.Children.Count - 1)
                    BreadcrumbTextBlock.Inlines.Add(new Run(" > "));
            }
        }

        private void Shell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                e.Handled = true;
                MainViewBack();
            }
        }

        private void Shell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1)
            {
                e.Handled = true;
                MainViewBack();
            }
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            var model = DataContext as ShellModel;
            model.ViewLoaded();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            //BookDownloadViewModel vm = new BookDownloadViewModel(BookIDTextBox.Text.Trim());
            //vm.ViewManager = this;
            //BookDownloadView view = new BookDownloadView();
            //view.DataContext = vm;
            //vm.Init(view);

            //MainViewGo(view);
        }
    }
}
