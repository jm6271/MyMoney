using MyMoney.Core.Models;
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
            if (value is string text)
            {
                string amount = text;
                if (text.StartsWith('$'))
                {
                    amount = text.Substring(1);
                }

                if (decimal.TryParse(amount, out decimal result))
                {
                    if (result >= 0)
                        return new Currency(result);
                }
                    
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
