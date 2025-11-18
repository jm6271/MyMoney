using System.Windows.Controls;
using System.Windows.Input;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for NewAccountDialog.xaml
    /// </summary>
    public partial class NewAccountDialog : ContentDialog
    {
        public NewAccountDialog(ContentPresenter dialogHost, NewAccountDialogViewModel viewModel)
            : base(dialogHost)
        {
            InitializeComponent();
            DataContext = viewModel;

            TxtAccountName.Focus();
        }

        private void TxtAccountName_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtAccountName.SelectAll();
        }

        private void txtStartingBalance_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtStartingBalance.MoveFocus(
                    new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next)
                );

                e.Handled = false;
            }
        }

        private void txtStartingBalance_GotFocus(object sender, RoutedEventArgs e)
        {
            txtStartingBalance.SelectAll();
        }

        private void TxtAccountName_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TxtAccountName.SelectAll();
        }

        private void txtStartingBalance_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            txtStartingBalance.SelectAll();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            TxtAccountName.Focus();
            TxtAccountName.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
