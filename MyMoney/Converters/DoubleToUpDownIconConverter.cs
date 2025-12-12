using System.Globalization;
using System.Windows.Data;

namespace MyMoney.Converters
{
    class DoubleToUpDownIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                // Return "ArrowDown24" for negative, "ArrowUp24" for positive or zero
                return percent < 0 ? "ArrowDown24" : "ArrowUp24";
            }

            // Fallback icon if value isn't a number
            return "QuestionCircle24";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
