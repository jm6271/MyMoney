namespace MyMoney.Views.Controls;

public class SelectedMonthChangedEventArgs(RoutedEvent routedEvent, object source, 
    DateTime newValue) : RoutedEventArgs(routedEvent, source)
{
    public DateTime NewValue { get; } = newValue;
}
