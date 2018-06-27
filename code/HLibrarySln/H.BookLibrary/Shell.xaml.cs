using H.Book;
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
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
            Loaded += Shell_Loaded;
        }

        private async void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            HBook book = new HBook(@"books\hitomi-1201123-2018-06-27.hb", HBookMode.Open);
            await book.InitAsync();
            BookDetailViewModel vm = new BookDetailViewModel(book);
            BookDetailView view = new BookDetailView();
            view.DataContext = vm;
            vm.Init(view);

            PageHost.Content = view;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string id = "1201123";
            HitomiBookDownloader bookDownloader = new HitomiBookDownloader();
            bookDownloader.Download(id, $"books/hitomi-{id}-{DateTime.Now.ToString("yyyy-MM-dd")}.hb");
        }
    }
}
