using MyMoney.ViewModels.ContentDialogs;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for BudgetCategoryDialog.xaml
    /// </summary>
    public partial class BudgetCategoryDialog : ContentDialog
    {
        public BudgetCategoryDialog(ContentPresenter dialogHost, BudgetCategoryDialogViewModel viewModel) : base(dialogHost)
        {
            InitializeComponent();
            DataContext = viewModel;
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

        private void txtAmount_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txtAmount.SelectAll();
        }

        private void TxtCategory_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TxtCategory.SelectAll();
        }
    }
}
