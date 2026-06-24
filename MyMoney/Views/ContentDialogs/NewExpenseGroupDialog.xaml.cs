using System.Windows.Input;
using MyMoney.Abstractions;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for NewExpenseGroupDialog.xaml
    /// </summary>
    public partial class NewExpenseGroupDialog : ContentDialog, IContentDialog
    {
        public NewExpenseGroupDialog()
        {
            InitializeComponent();
        }

        private void txtGroupName_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtGroupName.SelectAll();
        }

        private void txtGroupName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtGroupName.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = false;
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtGroupName.Focus();
            txtGroupName.SelectAll();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
                return;

            txtGroupName.Focus();
            txtGroupName.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            if (DataContext is NewExpenseGroupDialogViewModel vm)
            {
                vm.Validate();

                if (vm.HasErrors)
                {
                    args.Cancel = true;
                }
            }
        }
    }
}
