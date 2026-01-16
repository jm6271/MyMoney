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
using MyMoney.Abstractions;
using MyMoney.Core.Models;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
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

        public new async Task<ContentDialogResult> ShowAsync(CancellationToken cancellationToken = default)
        {
            if (DataContext is NewTransactionDialogViewModel viewModel)
            {
                txtPayee.Text = viewModel.NewTransactionPayee;
                cmbCategory.ItemsSource = viewModel.CategoryNames;
            }

            var result = await base.ShowAsync(cancellationToken);

            if (DataContext is NewTransactionDialogViewModel vm)
            {
                vm.NewTransactionPayee = SelectedPayee;
            }

            return result;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtAmount.Focus();
            txtAmount.SelectAll();
        }

        public string SelectedPayee
        {
            get { return txtPayee.Text; }
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

            // validate the user data, and if it is invalid, prevent the dialog from closing

            // get validation errors for all the required fields
            var amountValidationErrors = Validation.GetErrors(txtAmount);
            var dateValidationErrors = Validation.GetErrors(txtDate);
            var invalidPayee = txtPayee.Text == "";
            var invalidCategory = cmbCategory.SelectedIndex == -1;

            // Clear the red border from custom validated controls
            CategoryBorder.BorderBrush = Brushes.Transparent;
            PayeeBorder.BorderBrush = Brushes.Transparent;

            // validate
            if (
                !invalidPayee
                && !invalidCategory
                && amountValidationErrors is not { Count: > 0 }
                && dateValidationErrors is not { Count: > 0 }
            )
                return;
            args.Cancel = true;

            if (invalidCategory)
                CategoryBorder.BorderBrush = Brushes.Red;
            if (invalidPayee)
                PayeeBorder.BorderBrush = Brushes.Red;
        }

        private void TxtPayee_OnLostFocus(object sender, RoutedEventArgs e)
        {
            PayeeBorder.BorderBrush = string.IsNullOrEmpty(txtPayee.Text) ? Brushes.Red : Brushes.Transparent;
        }

        private void cmbCategory_SelectionChanged_1(object sender, RoutedEventArgs e)
        {
            CategoryBorder.BorderBrush = Brushes.Transparent;
        }

        private void cmbCategory_LostFocus(object sender, RoutedEventArgs e)
        {
            // Validate
            if (cmbCategory.SelectedIndex == -1)
            {
                CategoryBorder.BorderBrush = Brushes.Red;
            }
            else
            {
                CategoryBorder.BorderBrush = Brushes.Transparent;
            }
        }
    }
}
