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
    /// Interaction logic for NewExpenseGroupDialog.xaml
    /// </summary>
    public partial class NewExpenseGroupDialog : ContentDialog
    {
        public NewExpenseGroupDialog()
        {
            InitializeComponent();
        }

        private void txtGroupName_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtGroupName.SelectAll();
        }

        private void txtGroupName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtGroupName.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = false;
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtGroupName.Focus();
            txtGroupName.SelectAll();
        }
    }
}
