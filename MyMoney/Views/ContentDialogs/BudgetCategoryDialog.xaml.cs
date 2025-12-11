using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyMoney.Abstractions;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for BudgetCategoryDialog.xaml
    /// </summary>
    public partial class BudgetCategoryDialog : ContentDialog, IContentDialog
    {
        public BudgetCategoryDialog()
        {
            InitializeComponent();
            TxtCategory.Focus();
        }

        private void TxtCategory_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TxtCategory.SelectAll();
        }

        private void txtAmount_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtAmount.SelectAll();
        }

        private void txtAmount_KeyDown(object sender, KeyEventArgs e)
        {
            // tab to next control (OK button) and press enter on it (submit the dialog) when enter is pressed
            if (e.Key == Key.Enter)
            {
                txtAmount.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = false;
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
                return;
            TxtCategory.Focus();
            TxtCategory.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            var amountValidationErrors = Validation.GetErrors(txtAmount);

            if (!string.IsNullOrWhiteSpace(TxtCategory.Text) && amountValidationErrors is not { Count: > 0 })
                return;
            args.Cancel = true;

            ValidateTxtCategory();
        }

        private void ValidateTxtCategory()
        {
            if (string.IsNullOrWhiteSpace(TxtCategory.Text))
                TxtCategoryBorder.BorderBrush = new SolidColorBrush(Colors.Red);
            else
                TxtCategoryBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }

        private void TxtCategory_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ValidateTxtCategory();
        }

        private void TxtCategory_LostMouseCapture(object sender, MouseEventArgs e)
        {
            ValidateTxtCategory();
        }
    }
}
