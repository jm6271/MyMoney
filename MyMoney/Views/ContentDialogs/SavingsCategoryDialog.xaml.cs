using System.Windows.Input;
using MyMoney.Abstractions;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for SavingsCategoryDialog.xaml
    /// </summary>
    public partial class SavingsCategoryDialog : ContentDialog, IContentDialog
    {
        public SavingsCategoryDialog()
        {
            InitializeComponent();
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

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
                return;

            TxtCategory.Focus();
            TxtCategory.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            if (DataContext is SavingsCategoryDialogViewModel vm)
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
