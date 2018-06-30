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
        public BookDetailView()
        {
            InitializeComponent();
            Loaded += BookDetailView_Loaded;
            Unloaded += BookDetailView_Unloaded;

            Title = "书本概览";
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
        }

        private void BookDetailView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            DataContext = null;

            if (vm != null) vm.Release();
        }
    }
}
