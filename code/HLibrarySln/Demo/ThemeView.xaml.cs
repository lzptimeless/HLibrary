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

namespace Demo
{
    /// <summary>
    /// Interaction logic for ThemeView.xaml
    /// </summary>
    public partial class ThemeView : Window
    {
        public ThemeView()
        {
            InitializeComponent();
        }

        private void Enable_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                Root.IsEnabled = true;
        }

        private void Enable_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                Root.IsEnabled = false;
        }
    }
}
