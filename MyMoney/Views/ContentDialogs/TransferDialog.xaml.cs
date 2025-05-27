using MyMoney.ViewModels.ContentDialogs;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for TransferDialog.xaml
    /// </summary>
    public partial class TransferDialog : ContentDialog
    {
        public TransferDialog(ContentPresenter dialogHost, TransferDialogViewModel viewModel) : base(dialogHost)
        {
            InitializeComponent();
            DataContext = viewModel;
            cmbFrom.Focus();
        }

        private void txtAmount_GotFocus(object sender, RoutedEventArgs e)
        {
            txtAmount.SelectAll();
        }

        private void txtAmount_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtAmount.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = false;
            }
        }

        private void txtAmount_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            txtAmount.SelectAll();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary) return;

            // validate the user data, and if it is invalid, prevent the dialog from closing

            // get validation errors for all the required fields
            var amountValidationErrors = Validation.GetErrors(txtAmount);
            var fromHasErrors = cmbFrom.Text == "" || cmbFrom.Text == cmbTo.Text;
            var toHasErrors = cmbTo.Text == "" || cmbFrom.Text == cmbTo.Text;

            // Clear the red border from custom validated controls
            cmbFromBorder.BorderBrush = Brushes.Transparent;
            cmbToBorder.BorderBrush = Brushes.Transparent;

            // validate
            if (!fromHasErrors && !toHasErrors
                              && amountValidationErrors is not { Count: > 0 }) return;

            // Errors, prevent the dialog from cancelling
            args.Cancel = true;

            if (fromHasErrors)
                cmbFromBorder.BorderBrush = Brushes.Red;
            if (toHasErrors)
                cmbToBorder.BorderBrush = Brushes.Red;
        }

        private void cmbTo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cmbTo.IsDropDownOpen) return;
            cmbToBorder.BorderBrush = string.IsNullOrEmpty(cmbTo.Text) || cmbFrom.Text == cmbTo.Text ? Brushes.Red : Brushes.Transparent;
        }

        private void cmbFrom_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cmbFrom.IsDropDownOpen) return;
            cmbFromBorder.BorderBrush = string.IsNullOrEmpty(cmbFrom.Text) || cmbFrom.Text == cmbTo.Text ? Brushes.Red : Brushes.Transparent;
        }
    }
}
