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
    public partial class BookDownloadView : UserControl
    {
        public BookDownloadView()
        {
            InitializeComponent();
            Loaded += BookDownloadView_Loaded;
            Unloaded += BookDownloadView_Unloaded;
        }

        private void BookDownloadView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null) vm.ViewLoaded();
        }

        private void BookDownloadView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null) vm.Release();
        }

        public void Print(string msg)
        {
            OutputTextBox.AppendText(msg + Environment.NewLine);
        }
    }
}
