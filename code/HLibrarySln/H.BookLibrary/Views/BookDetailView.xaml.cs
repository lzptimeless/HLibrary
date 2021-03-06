﻿using H.BookLibrary.ViewModels;
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
    /// Interaction logic for BookDetailView.xaml
    /// </summary>
    public partial class BookDetailView : UserControl, IView
    {
        #region fields
        private Window _ownerWindow;
        #endregion

        public BookDetailView()
        {
            InitializeComponent();
            Loaded += BookDetailView_Loaded;
            Unloaded += BookDetailView_Unloaded;

            Title = "书本详细";
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
            var sv = UITree.FindDescendant<ScrollViewer>(PageListBox);
            if (sv != null) sv.ScrollToTop();
        }

        private void BookDetailView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null) vm.ViewLoaded();

            _ownerWindow = Window.GetWindow(this);
            _ownerWindow.MouseUp += _ownerWindow_MouseUp;
        }

        private void BookDetailView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            DataContext = null;

            if (vm != null) vm.Release();
            if (_ownerWindow != null) _ownerWindow.MouseUp -= _ownerWindow_MouseUp;
        }

        private void _ownerWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 页面不可见则不用处理窗口按键
            if (Visibility != Visibility.Visible) return;
            var vm = DataContext as BookDetailViewModel;

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
