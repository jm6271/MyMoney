using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyMoney.Behaviors
{
    public static class DoubleClickBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(DoubleClickBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(DoubleClickBehavior),
                new PropertyMetadata(null));

        public static ICommand GetCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value) =>
            obj.SetValue(CommandProperty, value);

        public static object GetCommandParameter(DependencyObject obj) =>
            obj.GetValue(CommandParameterProperty);

        public static void SetCommandParameter(DependencyObject obj, object value) =>
            obj.SetValue(CommandParameterProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                control.MouseDoubleClick -= OnMouseDoubleClick;
                if (e.NewValue is not null)
                    control.MouseDoubleClick += OnMouseDoubleClick;
            }
        }

        private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = (Control)sender;
            var command = GetCommand(control);
            var parameter = GetCommandParameter(control) ?? control.DataContext;

            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }
    }
}
