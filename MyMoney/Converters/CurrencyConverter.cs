using System.Globalization;
using System.Windows.Data;
using MyMoney.Core.Models;

namespace MyMoney.Converters
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
                try
                {
                    // Strip dollar signs
                    string expression = text.Replace("$", "").Trim();

                    // Strip commas
                    expression = expression.Replace(",", "");

                    // Evaluate with NCalc
                    var expr = new NCalc.Expression(expression);

                    var result = System.Convert.ToDecimal(expr.Evaluate());

                    if (result >= 0)
                        return new Currency(result);
                }
                catch (Exception)
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
