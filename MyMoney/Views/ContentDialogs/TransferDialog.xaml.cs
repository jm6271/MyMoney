using MyMoney.ViewModels.ContentDialogs;
using System.Windows.Controls;
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
    }
}
