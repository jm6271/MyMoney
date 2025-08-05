using MyMoney.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class SavingsCategoryDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private Currency _planned = new(0m);

        [ObservableProperty]
        private Currency _currentBalance = new(0m);

        [ObservableProperty]
        private ObservableCollection<Transaction> _recentTransactions = [];

        [ObservableProperty]
        private Visibility _recentTransactionsVisibility = Visibility.Visible;

        [ObservableProperty]
        private GridLength _recentTransactionsColumnWidth = new(1, GridUnitType.Star);

        [ObservableProperty]
        private GridLength _editColumnWidth = new(200, GridUnitType.Pixel);

        public void SortTransactions()
        {
            // Sort the transactions
            var sortedTransactions = RecentTransactions.OrderByDescending(x => x.Date).ToList();
            RecentTransactions.Clear();
            foreach (var transaction in sortedTransactions)
                RecentTransactions.Add(transaction);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(RecentTransactionsVisibility))
            {
                if (RecentTransactionsVisibility == Visibility.Collapsed)
                {
                    RecentTransactionsColumnWidth = new(0, GridUnitType.Pixel);
                    EditColumnWidth = new(1, GridUnitType.Star);
                }
                else
                {
                    RecentTransactionsColumnWidth = new(1, GridUnitType.Star);
                    EditColumnWidth = new(200, GridUnitType.Pixel);
                }
            }
        }
    }
}
