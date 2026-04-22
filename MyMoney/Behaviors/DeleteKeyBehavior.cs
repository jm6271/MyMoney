using System.Windows.Controls;
using System.Windows.Input;

namespace MyMoney.Behaviors
{
    public static class DeleteKeyBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(DeleteKeyBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value) =>
            obj.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                control.KeyDown -= OnKeyDown;
                if (e.NewValue is not null)
                    control.KeyDown += OnKeyDown;
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var control = (Control)sender;
                var command = GetCommand(control);
                if (command?.CanExecute(control.DataContext) == true)
                    command.Execute(control.DataContext);
            }
        }
    }
}

