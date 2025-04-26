using MyMoney.Core.Models;
using MyMoney.Views.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyMoney.Helpers
{
    internal class CategoryToGroupedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Category category)
            {
                GroupedComboBox.GroupedComboBoxItem item = new()
                {
                    Item = category.Name,
                    Group = category.Group,
                };

                return item;
            }
            return new GroupedComboBox.GroupedComboBoxItem();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GroupedComboBox.GroupedComboBoxItem item)
            {
                Category category = new()
                {
                    Group = item.Group,
                };

                string? str = item.Item.ToString();
                if (str != null) category.Name = str;

                return category;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
