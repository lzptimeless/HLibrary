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
    /// Interaction logic for BookDownloadView.xaml
    /// </summary>
    public partial class BookDownloadView : UserControl,IView
    {
        public BookDownloadView()
        {
            InitializeComponent();
            Loaded += BookDownloadView_Loaded;
            Unloaded += BookDownloadView_Unloaded;

            Title = "下载";
        }

        #region properties
        #region Title
        private string _title;
        /// <summary>
        /// Get or set <see cref="Title"/>
        /// </summary>
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }
        #endregion
        #endregion

        private void BookDownloadView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null) vm.ViewLoaded();
        }

        private void BookDownloadView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            DataContext = null;

            if (vm != null) vm.Release();
        }

        public void ClearPrint()
        {
            OutputTextBox.Clear();
        }

        public void Print(string msg)
        {
            OutputTextBox.AppendText(msg + Environment.NewLine);
            OutputTextBox.ScrollToEnd();
        }
    }
}
