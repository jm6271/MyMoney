using System.Collections;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyMoney.Converters
{
    public class FirstValidationErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable errors)
            {
                foreach (var error in errors)
                {
                    if (error is ValidationError validationError)
                    {
                        return validationError.ErrorContent?.ToString() ?? string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
