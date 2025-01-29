using MyMoney.Core.FS.Models;
using System.Globalization;
using System.Windows.Data;

namespace MyMoney.Helpers
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Currency currency) 
            { 
                return currency.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && decimal.TryParse(text, out decimal result))
            {
                return new Currency(result);
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
