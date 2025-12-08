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
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for UpdateAccountBalance.xaml
    /// </summary>
    public partial class UpdateAccountBalanceDialog : ContentDialog
    {
        public UpdateAccountBalanceDialog(ContentPresenter dialogHost, UpdateAccountBalanceDialogViewModel viewModel)
            : base(dialogHost)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtBalance.Focus();
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtBalance.SelectAll();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
                return;

            txtBalance.Focus();
            txtBalance.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            // validate the user data, and if it is invalid, prevent the dialog from closing
            var validationErrors = Validation.GetErrors(txtBalance);

            if (validationErrors is { Count: > 0 })
            {
                args.Cancel = true;
            }
        }
    }
}
