using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for SavingsCategoryDialog.xaml
    /// </summary>
    public partial class SavingsCategoryDialog : Wpf.Ui.Controls.ContentDialog
    {
        public SavingsCategoryDialog(ContentPresenter dialogHost, SavingsCategoryDialogViewModel viewModel)
            : base(dialogHost)
        {
            InitializeComponent();
            DataContext = viewModel;
            TxtCategory.Focus();
        }

        private void TxtCategory_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TxtCategory.SelectAll();
        }

        private void TxtPlanned_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TxtPlanned.SelectAll();
        }

        private void TxtBalance_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TxtBalance.SelectAll();
        }

        private void ContentDialog_Closing(
            Wpf.Ui.Controls.ContentDialog sender,
            Wpf.Ui.Controls.ContentDialogClosingEventArgs args
        )
        {
            if (args.Result != ContentDialogResult.Primary)
                return;

            TxtCategory.Focus();
            TxtCategory.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            // validate the user data, and if it is invalid, prevent the dialog from closing

            // get validation errors for all the required fields
            var plannedValidationErrors = Validation.GetErrors(TxtPlanned);
            var balanceValidationErrors = Validation.GetErrors(TxtBalance);
            var invalidCategory = string.IsNullOrEmpty(TxtCategory.Text);

            // Clear the red border from custom validated controls
            TxtCategoryBorder.BorderBrush = Brushes.Transparent;

            // validate
            if (
                !invalidCategory
                && plannedValidationErrors is not { Count: > 0 }
                && balanceValidationErrors is not { Count: > 0 }
            )
                return;
            args.Cancel = true;

            if (invalidCategory)
                TxtCategoryBorder.BorderBrush = Brushes.Red;
        }

        private void TxtCategory_LostFocus(object sender, RoutedEventArgs e)
        {
            TxtCategoryBorder.BorderBrush = string.IsNullOrEmpty(TxtCategory.Text) ? Brushes.Red : Brushes.Transparent;
        }
    }
}
