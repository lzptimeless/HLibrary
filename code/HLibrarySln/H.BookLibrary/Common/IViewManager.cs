using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace H.BookLibrary
{
    public interface IViewManager
    {
        void MainViewSet(IView view);
        void MainViewForward(IView view);
        void MainViewBack();
    }

    public interface IView
    {
        string Title { get; }
    }
}
