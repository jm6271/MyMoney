using System.Windows.Controls;
using System.Windows.Input;
using MyMoney.Abstractions;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for UpdateAccountBalance.xaml
    /// </summary>
    public partial class UpdateAccountBalanceDialog : ContentDialog, IContentDialog
    {
        public UpdateAccountBalanceDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtBalance.Focus();
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtBalance.SelectAll();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
                return;

            txtBalance.Focus();
            txtBalance.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            // validate the user data, and if it is invalid, prevent the dialog from closing
            var validationErrors = Validation.GetErrors(txtBalance);

            if (validationErrors is { Count: > 0 })
            {
                args.Cancel = true;
            }
        }
    }
}
