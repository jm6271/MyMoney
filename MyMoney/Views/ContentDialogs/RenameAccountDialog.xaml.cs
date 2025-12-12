using MyMoney.Abstractions;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for RenameAccountDialog.xaml
    /// </summary>
    public partial class RenameAccountDialog : ContentDialog, IContentDialog
    {
        public RenameAccountDialog()
        {
            InitializeComponent();
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
