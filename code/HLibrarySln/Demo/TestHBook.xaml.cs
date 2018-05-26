using Microsoft.Win32;
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

namespace Demo
{
    /// <summary>
    /// Interaction logic for TestHBook.xaml
    /// </summary>
    public partial class TestHBook : UserControl
    {
        public TestHBook()
        {
            InitializeComponent();
        }

        private void CoverThumbButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TestHBookViewModel;

            OpenFileDialog dg = new OpenFileDialog();
            dg.Filter = "图片|*.jpg;*.png;*.gif";
            if (dg.ShowDialog() == false) return;

            vm.InputCoverThumb = dg.FileName;
        }

        private void CoverButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TestHBookViewModel;

            OpenFileDialog dg = new OpenFileDialog();
            dg.Filter = "图片|*.jpg;*.png;*.gif";
            if (dg.ShowDialog() == false) return;

            vm.InputCover = dg.FileName;
        }

        private void PageThumbButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TestHBookViewModel;

            OpenFileDialog dg = new OpenFileDialog();
            dg.Filter = "图片|*.jpg;*.png;*.gif";
            if (dg.ShowDialog() == false) return;

            vm.InputPageThumb = dg.FileName;
        }

        private void PageContentButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TestHBookViewModel;

            OpenFileDialog dg = new OpenFileDialog();
            dg.Filter = "图片|*.jpg;*.png;*.gif";
            if (dg.ShowDialog() == false) return;

            vm.InputPageContent = dg.FileName;
        }
    }
}
