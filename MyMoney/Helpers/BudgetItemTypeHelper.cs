using System.Windows;

namespace MyMoney.Helpers
{
    public static class BudgetItemTypeHelper
    {
        public static readonly DependencyProperty ItemTypeProperty =
            DependencyProperty.RegisterAttached(
                "ItemType",
                typeof(string),
                typeof(BudgetItemTypeHelper),
                new PropertyMetadata("Income"));

        public static string GetItemType(DependencyObject obj)
        {
            return (string)obj.GetValue(ItemTypeProperty);
        }

        public static void SetItemType(DependencyObject obj, string value)
        {
            obj.SetValue(ItemTypeProperty, value);
        }
    }
}