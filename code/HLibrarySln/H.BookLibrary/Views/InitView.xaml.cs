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
    /// Interaction logic for InitView.xaml
    /// </summary>
    public partial class InitView : UserControl, IView
    {
        public InitView()
        {
            InitializeComponent();
            Loaded += InitView_Loaded;
            Unloaded += InitView_Unloaded;
        }

        public string Title { get { return "初始化"; } }

        private void InitView_Loaded(object sender, RoutedEventArgs e)
        {
            var model = DataContext as ViewModelBase;
            if (model != null) model.ViewLoaded();
        }

        private void InitView_Unloaded(object sender, RoutedEventArgs e)
        {
            var model = DataContext as ViewModelBase;
            if (model != null) model.Release();
        }
    }
}
