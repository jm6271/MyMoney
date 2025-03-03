using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for NewTransactionDialog.xaml
    /// </summary>
    public partial class NewTransactionDialog : ContentDialog
    {
        public NewTransactionDialog(ContentPresenter dialogHost, AccountsViewModel viewModel) : base(dialogHost)
        {
            InitializeComponent();
            DataContext = viewModel;
            txtPayee.Text = viewModel.NewTransactionPayee;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtAmount.Focus();
            txtAmount.SelectAll();
        }

        public string SelectedPayee
        {
            get
            {
                return txtPayee.Text;
            }
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

        private void txtAmount_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtAmount.SelectAll();
        }

        private void txtMemo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtMemo.SelectAll();
        }

        private void txtMemo_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtMemo.SelectAll();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary) return;

            // validate the user data, and if it is invalid, prevent the dialog from closing

            // get validation errors for all the required fields
            var amountValidationErrors = Validation.GetErrors(txtAmount);
            var dateValidationErrors = Validation.GetErrors(txtDate);
            bool invalidPayee = txtPayee.Text == "";
            bool invalidCategory = cmbCategory.Text == "";

            // validate
            if (invalidPayee || invalidCategory
                || (amountValidationErrors != null && amountValidationErrors.Count > 0)
                || (dateValidationErrors != null && dateValidationErrors.Count > 0))
            {
                args.Cancel = true;
            }
        }
    }
}
