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
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string id = "894871";
            HitomiBookDownloader bookDownloader = new HitomiBookDownloader();
            bookDownloader.Download(id, $"books/hitomi-{id}-{DateTime.Now.ToString("yyyy-MM-dd")}");
        }
    }
}
