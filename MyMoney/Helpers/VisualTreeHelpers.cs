using System.Windows.Media;

namespace MyMoney.Helpers
{
    public static class VisualTreeHelpers
    {
        public static T? FindAncestor<T>(this DependencyObject current)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T correctlyTyped)
                    return correctlyTyped;

                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
