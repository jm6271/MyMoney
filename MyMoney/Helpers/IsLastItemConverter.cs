using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyMoney.Helpers
{
    public class IsLastItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = current item,
            // values[1] = the IEnumerable (ItemsSource)
            var item = values[0];
            if (values[1] is not IEnumerable items) return false;

            // grab the last item in the enumeration:
            var last = items.Cast<object>().LastOrDefault();
            return object.Equals(item, last);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
