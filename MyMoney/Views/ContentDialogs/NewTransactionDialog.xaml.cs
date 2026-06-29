using System.Windows.Input;
using MyMoney.Abstractions;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for NewTransactionDialog.xaml
    /// </summary>
    public partial class NewTransactionDialog : ContentDialog, IContentDialog
    {
        public NewTransactionDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtAmount.Focus();
            txtAmount.SelectAll();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // tab to next control (OK button) and press enter on it (submit the dialog) when enter is pressed
            if (e.Key == Key.Enter)
            {
                txtMemo.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = false;
            }
        }

        private void txtAmount_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtAmount.SelectAll();
        }

        private void txtMemo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtMemo.SelectAll();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
                return;

            txtPayee.Focus();
            txtPayee.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            if (DataContext is NewTransactionDialogViewModel vm)
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
