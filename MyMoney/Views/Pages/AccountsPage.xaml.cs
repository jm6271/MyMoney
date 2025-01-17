using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class AccountsPage : Page
    {
        public AccountsViewModel ViewModel { get; }

        public AccountsPage(AccountsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            if (cmbAccounts.Items.Count > 0)
            {
                cmbAccounts.SelectedIndex = 0;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();

            TransactionsList.MaxHeight = Application.Current.MainWindow.ActualHeight - 300;
        }
    }
}
