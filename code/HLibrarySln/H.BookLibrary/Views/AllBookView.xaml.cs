using H.BookLibrary.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H.BookLibrary.Views
{
    /// <summary>
    /// Interaction logic for AllBookView.xaml
    /// </summary>
    public partial class AllBookView : UserControl, IView
    {
        #region fields
        private Window _ownerWindow;
        #endregion

        public AllBookView()
        {
            InitializeComponent();

            Loaded += AllBookView_Loaded;
            Unloaded += AllBookView_Unloaded;

            Title = "书本浏览";
        }

        #region properties
        #region Title
        private string _title;
        /// <summary>
        /// Get or set <see cref="Title"/>
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        #endregion
        #endregion

        public void PagesScrollToTop()
        {
            var sv = UITree.FindDescendant<ScrollViewer>(BookListBox);
            if (sv != null) sv.ScrollToTop();
        }

        private void AllBookView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null) vm.ViewLoaded();

            _ownerWindow = Window.GetWindow(this);
            _ownerWindow.MouseUp += _ownerWindow_MouseUp;
        }

        private void AllBookView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            DataContext = null;

            if (vm != null) vm.Release();
            if (vm != null) vm.Release();
            if (_ownerWindow != null) _ownerWindow.MouseUp -= _ownerWindow_MouseUp;
        }

        private void _ownerWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 页面不可见则不用处理窗口按键
            if (Visibility != Visibility.Visible) return;
            var vm = DataContext as AllBookViewModel;

            if (vm == null)
            {
                Output.Print("ViewModel is null.");
                return;
            }

            if (e.ChangedButton == MouseButton.XButton1)
            {
                if (vm.PreGalleryPageCommand.CanExecute(null))
                {
                    e.Handled = true;
                    vm.PreGalleryPageCommand.Execute(null);
                }
            }
            else if (e.ChangedButton == MouseButton.XButton2)
            {
                if (vm.NextGalleryPageCommand.CanExecute(null))
                {
                    e.Handled = true;
                    vm.NextGalleryPageCommand.Execute(null);
                }
            }
        }
    }
}
