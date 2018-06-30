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
    public partial class PageGalleryView : UserControl, IView
    {
        #region fields
        private Window _ownerWindow;
        #endregion

        public PageGalleryView()
        {
            InitializeComponent();

            Loaded += PageGalleryView_Loaded;
            Unloaded += PageGalleryView_Unloaded;

            Title = "页面浏览";
        }

        #region properties
        #region Title
        private string _title;
        /// <summary>
        /// Get or set <see cref="Title"/>
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        #endregion
        #endregion

        public void SetPageImage(ImageSource src, Stretch stretch)
        {
            if (src == null)
            {
                Canvas.SetLeft(PageImageHost, 0);
                Canvas.SetTop(PageImageHost, 0);
                PageImage.Source = null;
                PageImage.Stretch = Stretch.None;
                PageImageHost.Width = double.NaN;
                PageImageHost.Height = double.NaN;
                return;
            }

            double canvasWidth = PageImageCanvas.ActualWidth;
            double canvasHeight = PageImageCanvas.ActualHeight;

            if (stretch == Stretch.None)
            {
                PageImageHost.Width = src.Width;
                PageImageHost.Height = src.Height;
            }
            else if (stretch == Stretch.Fill)
            {
                PageImageHost.Width = canvasWidth;
                PageImageHost.Height = canvasHeight;
            }
            else if (stretch == Stretch.Uniform)
            {
                PageImageHost.Width = canvasWidth;
                PageImageHost.Height = canvasHeight;
            }
            else if (stretch == Stretch.UniformToFill)
            {
                double srcRate = src.Width / src.Height;
                double canvasRate = canvasWidth / canvasHeight;
                if (canvasRate > srcRate)
                {
                    PageImageHost.Width = canvasWidth;
                    PageImageHost.Height = canvasWidth / srcRate;
                }
                else
                {
                    PageImageHost.Height = canvasHeight;
                    PageImageHost.Width = canvasHeight * srcRate;
                }
            }

            if (PageImageHost.Width < canvasWidth)
                Canvas.SetLeft(PageImageHost, (canvasWidth - PageImageHost.Width) / 2);
            else
                Canvas.SetLeft(PageImageHost, 0);

            if (PageImageHost.Height < canvasHeight)
                Canvas.SetTop(PageImageHost, (canvasHeight - PageImageHost.Height) / 2);
            else
                Canvas.SetTop(PageImageHost, 0);

            PageImage.Stretch = stretch;
            PageImage.Source = src;
        }

        private void PageGalleryView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null) vm.ViewLoaded();

            _ownerWindow = Window.GetWindow(this);
            _ownerWindow.KeyUp += _ownerWindow_KeyUp;
        }

        private void _ownerWindow_KeyUp(object sender, KeyEventArgs e)
        {
            // 页面不可见则不用处理窗口按键
            if (Visibility != Visibility.Visible) return;

            if (e.Key == Key.Space)
            {
                double imgWidth = PageImageHost.Width;
                double imgHeight = PageImageHost.Height;
                double canvasWidth = PageImageCanvas.ActualWidth;
                double canvasHeight = PageImageCanvas.ActualHeight;

                if (!double.IsNaN(imgWidth) && !double.IsNaN(imgHeight))
                {
                    if (imgHeight > canvasHeight)
                    {
                        e.Handled = true;
                        if (0 == MovePageImageLocation(0, -canvasHeight / 2))
                            MovePageImageLocation(0, imgHeight);
                    }
                    else if (imgWidth > canvasWidth)
                    {
                        e.Handled = true;
                        if (0 == MovePageImageLocation(-canvasWidth / 2, 0))
                            MovePageImageLocation(imgWidth, 0);
                    }
                }
            }// if Key.Space
            else if (e.Key == Key.Left)
            {
                var vm = DataContext as PageGalleryViewModel;
                if (vm != null && vm.PrePageCommand.CanExecute(null))
                {
                    e.Handled = true;
                    vm.PrePageCommand.Execute(null);
                }

                // 防止焦点乱跑触发了别的控件事件
                if (!PageDragThumb.IsFocused) PageDragThumb.Focus();
            }
            else if (e.Key == Key.Right)
            {
                var vm = DataContext as PageGalleryViewModel;
                if (vm != null && vm.NextPageCommand.CanExecute(null))
                {
                    e.Handled = true;
                    vm.NextPageCommand.Execute(null);
                }

                // 防止焦点乱跑触发了别的控件事件
                if (!PageDragThumb.IsFocused) PageDragThumb.Focus();
            }
        }

        private void PageGalleryView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            DataContext = null;

            if (vm != null) vm.Release();
            if (_ownerWindow != null) _ownerWindow.KeyUp -= _ownerWindow_KeyUp;
        }

        private void PageDragThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            MovePageImageLocation(e.HorizontalChange, e.VerticalChange);
        }

        private void PageDragThumb_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double imgWidth = PageImageHost.Width;
            double imgHeight = PageImageHost.Height;

            if (double.IsNaN(imgWidth) || double.IsNaN(imgHeight)) return;

            double canvasWidth = PageImageCanvas.ActualWidth;
            double canvasHeight = PageImageCanvas.ActualHeight;

            if (imgHeight > canvasHeight)
                MovePageImageLocation(0, e.Delta);
            else if (imgWidth > canvasWidth)
                MovePageImageLocation(e.Delta, 0);

            return;
        }

        private double MovePageImageLocation(double xDelta, double yDelta)
        {
            double currentX = Canvas.GetLeft(PageImageHost);
            double currentY = Canvas.GetTop(PageImageHost);
            double imgWidth = PageImageHost.Width;
            double imgHeight = PageImageHost.Height;

            if (double.IsNaN(imgWidth) || double.IsNaN(imgHeight)) return 0;

            double canvasWidth = PageImageCanvas.ActualWidth;
            double canvasHeight = PageImageCanvas.ActualHeight;

            double newX = 0, newY = 0;
            if (imgWidth > canvasWidth)
            {
                newX = Math.Max(canvasWidth - imgWidth, Math.Min(0, currentX + xDelta));
                Canvas.SetLeft(PageImageHost, newX);
            }

            if (imgHeight > canvasHeight)
            {
                newY = Math.Max(canvasHeight - imgHeight, Math.Min(0, currentY + yDelta));
                Canvas.SetTop(PageImageHost, newY);
            }

            return newY - currentY != 0 ? newY - currentY : newX - currentX;
        }

        private void PageImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PageImage.Source != null)
                SetPageImage(PageImage.Source, PageImage.Stretch);
        }
    }
}
