﻿using H.Book;
using H.BookLibrary.ViewModels;
using H.BookLibrary.Views;
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
            Loaded += Shell_Loaded;
        }

        public void MainViewGo(FrameworkElement view)
        {
            if (view == null) throw new ArgumentNullException("view");

            // 隐藏当前页面
            if (PageHost.Children.Count > 0)
            {
                var currentView = PageHost.Children[PageHost.Children.Count - 1];
                currentView.Visibility = Visibility.Collapsed;
            }

            PageHost.Children.Add(view);
        }

        public void MainViewBack()
        {
            // 移除当前页面
            if (PageHost.Children.Count > 0)
                PageHost.Children.RemoveAt(PageHost.Children.Count - 1);

            // 显示前一个页面
            if (PageHost.Children.Count > 0)
            {
                var preView = PageHost.Children[PageHost.Children.Count - 1];
                preView.Visibility = Visibility.Visible;
            }
        }

        private async void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            HBook book = new HBook(@"books\hitomi-1201123-2018-06-27.hb", HBookMode.Open);
            await book.InitAsync();
            BookDetailViewModel vm = new BookDetailViewModel(book);
            vm.ViewManager = this;
            BookDetailView view = new BookDetailView();
            view.DataContext = vm;
            vm.Init(view);

            MainViewGo(view);
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
