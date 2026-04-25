using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MyMoney.Core.Models;

namespace MyMoney.Converters
{
    public class BudgetItemProgressBarBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not Currency budgetedAmount || values[1] is not Currency actualAmount)
            {
                return (SolidColorBrush)Application.Current.Resources["PositiveForegroundColorBrush"];
            }

            // Get the item type from the third binding or from the parameter
            string? itemType = parameter as string;
            if (values.Length >= 3 && values[2] is string typeFromBinding)
            {
                itemType = typeFromBinding;
            }

            var isExpenseItem = itemType?.Equals("Expense", StringComparison.OrdinalIgnoreCase) ?? false;

            var isOverspent = isExpenseItem && actualAmount.Value > budgetedAmount.Value;
            if (isOverspent)
            {
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x55, 0x55));
            }

            return (SolidColorBrush)Application.Current.Resources["PositiveForegroundColorBrush"];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
