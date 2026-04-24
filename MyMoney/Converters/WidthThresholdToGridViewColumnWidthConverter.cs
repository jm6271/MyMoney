using System;
using System.Globalization;
using System.Windows.Data;

namespace MyMoney.Converters
{
    public class WidthThresholdToGridViewColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double currentWidth)
            {
                return double.NaN;
            }

            // GridViewColumn uses NaN as "Auto".
            return currentWidth < 600 ? 0d : double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
