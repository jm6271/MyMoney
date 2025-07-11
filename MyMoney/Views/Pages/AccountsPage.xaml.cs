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
        // Cache the “wide” padding so we can restore it
        private Thickness _wideTransactionMargin;
        private Thickness _wideAccountsMargin;

        // Define the “narrow” padding (no left inset)
        private readonly Thickness _narrowMargin = new Thickness(0, 0, 0, 24);

        public AccountsViewModel ViewModel { get; }

        public AccountsPage(AccountsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            _wideTransactionMargin = TransactionsGrid.Margin;
            _wideAccountsMargin = AccountsCard.Margin;

            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 750)
            {
                // stacked
                Col0.Width = new GridLength(0);
                Grid.SetColumn(AccountsCard, 1);
                Grid.SetRow(AccountsCard, 0);
                Grid.SetRow(TransactionsGrid, 1);
                TransactionsGrid.Margin = _narrowMargin;
                AccountsCard.Margin = _narrowMargin;
            }
            else
            {
                // side‑by‑side: first fixed, second star
                Col0.Width = new GridLength(275);
                Grid.SetColumn(AccountsCard, 0);
                Grid.SetRow(AccountsCard, 0);
                Grid.SetRow(TransactionsGrid, 0);
                TransactionsGrid.Margin = _wideTransactionMargin;
                AccountsCard.Margin = _wideAccountsMargin;
            }

            UpdateTransactionsMaxHeight();
        }

        private void UpdateTransactionsMaxHeight()
        {
            TransactionsList.MaxHeight = Application.Current.MainWindow.ActualHeight - 220;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();

            UpdateTransactionsMaxHeight();
        }
    }
}
