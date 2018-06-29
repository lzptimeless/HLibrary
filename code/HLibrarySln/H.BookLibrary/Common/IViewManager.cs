using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace H.BookLibrary
{
    public interface IViewManager
    {
        void MainViewSet(FrameworkElement view);
        void MainViewForward(FrameworkElement view);
        void MainViewBack();
    }
}
