using System.Windows.Input;
using MyMoney.Abstractions;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for TransferDialog.xaml
    /// </summary>
    public partial class TransferDialog : ContentDialog, IContentDialog
    {
        public TransferDialog()
        {
            InitializeComponent();
            cmbFrom.Focus();
        }

        private void txtAmount_GotFocus(object sender, RoutedEventArgs e)
        {
            txtAmount.SelectAll();
        }

        private void txtAmount_KeyDown(object sender, KeyEventArgs e)
        {
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

            txtAmount.Focus();
            txtAmount.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            if (DataContext is TransferDialogViewModel vm)
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
