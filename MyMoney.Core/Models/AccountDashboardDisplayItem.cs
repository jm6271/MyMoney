using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class AccountDashboardDisplayItem : ObservableObject
    {
        [ObservableProperty]
        private string _AccountName = "";

        [ObservableProperty]
        private Currency _Total = new(0m);

        public AccountDashboardDisplayItem()
        {

        }

        public AccountDashboardDisplayItem(string accountName, Currency total)
        {
            _AccountName = accountName;
            _Total = total;
        }

        public AccountDashboardDisplayItem(Account account)
        {
            _AccountName = account.AccountName;
            _Total = account.Total;
        }
    }
}
