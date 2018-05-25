using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Demo.Converters
{
    public class ArrayToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder sb = new StringBuilder();
            if (value is IEnumerable)
            {
                foreach (var item in value as IEnumerable)
                {
                    sb.Append(item != null ? item.ToString() : string.Empty).Append(',');
                }
                if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
