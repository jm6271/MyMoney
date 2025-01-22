using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui;
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

            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;

            if (cmbAccounts.Items.Count > 0)
            {
                cmbAccounts.SelectedIndex = 0;
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTransactionsMaxHeight();
        }

        private void UpdateTransactionsMaxHeight()
        {
            TransactionsList.MaxHeight = Application.Current.MainWindow.ActualHeight - 270;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();

            UpdateTransactionsMaxHeight();
        }
    }
}
