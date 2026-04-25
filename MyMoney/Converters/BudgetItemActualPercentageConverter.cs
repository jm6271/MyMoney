using System.Globalization;
using System.Windows.Data;
using MyMoney.Core.Models;

namespace MyMoney.Converters
{
    public class BudgetItemActualPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not Currency budgetedAmount || values[1] is not Currency actualAmount)
            {
                return 0d;
            }

            if (budgetedAmount.Value == 0m)
            {
                return 0d;
            }

            var percentage = (double)(actualAmount.Value / budgetedAmount.Value) * 100d;
            return Math.Clamp(percentage, 0d, 100d);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
