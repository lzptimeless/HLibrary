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
        void MainViewGo(FrameworkElement view);
        void MainViewBack();
    }
}
