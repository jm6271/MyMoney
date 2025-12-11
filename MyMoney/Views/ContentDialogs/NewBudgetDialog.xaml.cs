using System.Windows.Controls;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;
using MyMoney.Abstractions;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for NewBudgetDialog.xaml
    /// </summary>
    public partial class NewBudgetDialog : ContentDialog, IContentDialog
    {
        public NewBudgetDialog()
        {
            InitializeComponent();
            cmbBudgetDates.Focus();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (cmbBudgetDates.Items.Count > 0)
            {
                cmbBudgetDates.SelectedIndex = cmbBudgetDates.Items.Count - 1;
            }
        }
    }
}
