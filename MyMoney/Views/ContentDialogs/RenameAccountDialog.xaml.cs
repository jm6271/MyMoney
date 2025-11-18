using System;
using System.Windows.Controls;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for RenameAccountDialog.xaml
    /// </summary>
    public partial class RenameAccountDialog : ContentDialog
    {
        public RenameAccountDialog(ContentPresenter dialogHost, RenameAccountViewModel viewModel)
            : base(dialogHost)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtNewName.SelectAll();
            txtNewName.Focus();
        }

        private void txtNewName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtNewName.MoveFocus(
                    new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next)
                );
                e.Handled = false;
            }
        }
    }
}
