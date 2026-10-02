using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Zorbo.Server.WPF
{
    public static partial class Extensions
    {
        public static int Count(this IEnumerable enumerable) {
            int count = 0;

            foreach (var item in enumerable)
                count++;

            return count;
        }

        public static T FindVisualAnscestor<T>(this DependencyObject element) where T : DependencyObject {
            T ret = null;
            DependencyObject obj = element;

            do {
                obj = VisualTreeHelper.GetParent(obj);
                if (obj is T t) {
                    ret = t;
                    break;
                }
            }
            while (obj != null);

            if (ret != null) return ret;

            return null;
        }
    }
}
