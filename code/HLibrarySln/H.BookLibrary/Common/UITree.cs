using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace H.BookLibrary
{
    public static class UITree
    {
        public static T FindDescendant<T>(this DependencyObject element) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(element);
            if (count == 0) return null;

            DependencyObject[] childs = new DependencyObject[count];
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is T) return child as T;

                childs[i] = child;
            }

            foreach (var child in childs)
            {
                var res = FindDescendant<T>(child);
                if (res != null) return res;
            }

            return null;
        }
    }
}
