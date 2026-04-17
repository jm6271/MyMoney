using System.Windows.Controls;
using System.Windows.Media;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class AccountsPage : INavigableView<AccountsViewModel>
    {
        bool _isLoaded = false;

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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();
            _isLoaded = true;
        }

        private async void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                ResetScroll(TransactionsList);
            }

            await ViewModel.SelectedAccountChanged();
        }

        private void ResetScroll(System.Windows.Controls.ListView list)
        {
            var scroll = FindScrollViewer(list);
            scroll?.ScrollToTop();
        }

        private ScrollViewer? FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer sv) return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                var result = FindScrollViewer(child);
                if (result != null) return result;
            }

            return null;
        }

        private async void TransactionsList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is not ScrollViewer viewer) return;

            // when within 200px of bottom
            if (viewer.VerticalOffset + viewer.ViewportHeight >= viewer.ExtentHeight - 200)
            {
                await ViewModel.LoadTransactions();
            }
        }
    }
}
