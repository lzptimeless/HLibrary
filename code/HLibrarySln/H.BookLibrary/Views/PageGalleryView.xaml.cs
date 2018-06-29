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
    /// Interaction logic for PageGalleryView.xaml
    /// </summary>
    public partial class PageGalleryView : UserControl
    {
        public PageGalleryView()
        {
            InitializeComponent();

            Loaded += PageGalleryView_Loaded;
            Unloaded += PageGalleryView_Unloaded;
        }

        public void SetPageImage(ImageSource src, Stretch stretch)
        {
            // 重置画面位置
            Canvas.SetLeft(PageImageHost, 0);
            Canvas.SetTop(PageImageHost, 0);

            if (src == null)
            {
                PageImage.Source = src;
                PageImage.Stretch = Stretch.None;
                PageImageHost.Width = double.NaN;
                PageImageHost.Height = double.NaN;
                return;
            }

            if (stretch == Stretch.None)
            {
                PageImageHost.Width = src.Width;
                PageImageHost.Height = src.Height;
            }
            else if (stretch == Stretch.Fill)
            {
                PageImageHost.Width = PageImageCanvas.ActualWidth;
                PageImageHost.Height = PageImageCanvas.ActualHeight;
            }
            else if (stretch == Stretch.Uniform)
            {
                PageImageHost.Width = PageImageCanvas.ActualWidth;
                PageImageHost.Height = PageImageCanvas.ActualHeight;
            }
            else if (stretch == Stretch.UniformToFill)
            {
                double srcRate = src.Width / src.Height;
                double canvasRate = PageImageCanvas.ActualWidth / PageImageCanvas.ActualHeight;
                if (canvasRate > srcRate)
                {
                    PageImageHost.Width = PageImageCanvas.ActualWidth;
                    PageImageHost.Height = PageImageCanvas.ActualWidth / srcRate;
                }
                else
                {
                    PageImageHost.Height = PageImageCanvas.Height;
                    PageImageHost.Width = PageImageCanvas.Height * srcRate;
                }
            }

            PageImage.Stretch = stretch;
            PageImage.Source = src;
        }

        private void PageGalleryView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null) vm.ViewLoaded();
        }

        private void PageGalleryView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            DataContext = null;

            if (vm != null) vm.Release();
        }

        private void PageDragThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double currentX = Canvas.GetLeft(PageImageHost);
            double currentY = Canvas.GetTop(PageImageHost);

            Canvas.SetLeft(PageImageHost, currentX + e.HorizontalChange);
            Canvas.SetTop(PageImageHost, currentY + e.VerticalChange);
        }

        private void PageImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PageImage.Source != null)
                SetPageImage(PageImage.Source, PageImage.Stretch);
        }
    }
}
