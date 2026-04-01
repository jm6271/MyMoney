using System.Globalization;
using System.Windows.Data;

namespace MyMoney.Views.Controls;

/// <summary>
/// Converts a DateTime to a formatted string using the format string passed as the converter parameter.
/// Example: parameter="MMM" → "Jan", parameter="yyyy" → "2025"
/// </summary>
public class MonthYearConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dt && parameter is string format)
            return dt.ToString(format, culture ?? CultureInfo.CurrentCulture);

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
