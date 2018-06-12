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

            List<DemoData> datas = new List<DemoData>(new []{
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" },
                new DemoData { Property1="属性1", Property2="属性2",Property3="属性3",Property4="属性4",Property5="属性5" }
            });
            DataGrid.ItemsSource = datas;
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

        class DemoData
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
        }
    }
}
