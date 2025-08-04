using Wpf.Ui.Controls;

namespace MyMoney.Helpers
{
    public static class CardExpanderStateHelper
    {
        public static readonly DependencyProperty SyncedIsExpandedProperty =
            DependencyProperty.RegisterAttached(
                "SyncedIsExpanded",
                typeof(bool),
                typeof(CardExpanderStateHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSyncedIsExpandedChanged));

        public static void SetSyncedIsExpanded(DependencyObject element, bool value)
            => element.SetValue(SyncedIsExpandedProperty, value);

        public static bool GetSyncedIsExpanded(DependencyObject element)
            => (bool)element.GetValue(SyncedIsExpandedProperty);

        private static void OnSyncedIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CardExpander expander)
            {
                // Prevent recursive updates
                if (expander.IsExpanded != (bool)e.NewValue)
                    expander.IsExpanded = (bool)e.NewValue;

                expander.Expanded -= Expander_Expanded;
                expander.Collapsed -= Expander_Collapsed;

                expander.Expanded += Expander_Expanded;
                expander.Collapsed += Expander_Collapsed;
            }
        }

        private static void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is CardExpander expander)
                SetSyncedIsExpanded(expander, true);
        }

        private static void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (sender is CardExpander expander)
                SetSyncedIsExpanded(expander, false);
        }
    }

}
