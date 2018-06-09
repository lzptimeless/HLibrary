using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Selection.ItemsSource = new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Item8", "Item9", "Item10", "Item11", "Item12", "Item13", "Item14", "Item15", "Item16", "Item17", "Item18", "Item19", "Item20" };
            Selection.SelectedIndex = 2;
            //Selection.IsEnabled = false;
            Selection2.ItemsSource = new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Item8", "Item9", "Item10", "Item11", "Item12", "Item13", "Item14", "Item15", "Item16", "Item17", "Item18", "Item19", "Item20" };
            Selection2.SelectedIndex = 2;
            List.ItemsSource = new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Item8", "Item9", "Item10", "Item11", "Item12", "Item13", "Item14", "Item15", "Item16", "Item17", "Item18", "Item19", "Item20" };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //var vm = new TestHBookViewModel();
            //TestHBookView.DataContext = vm;
            //await vm.Init();
        }
    }
}
