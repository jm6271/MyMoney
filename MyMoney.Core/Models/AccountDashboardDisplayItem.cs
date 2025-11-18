using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class AccountDashboardDisplayItem : ObservableObject
    {
        [ObservableProperty]
        private string _accountName = "";

        [ObservableProperty]
        private Currency _total = new Currency(0m);

        public static AccountDashboardDisplayItem FromAccount(Account account)
        {
            var displayItem = new AccountDashboardDisplayItem();
            displayItem.AccountName = account.AccountName;
            displayItem.Total = account.Total;
            return displayItem;
        }

        public static AccountDashboardDisplayItem FromInitializers(string accountName, Currency total)
        {
            var displayItem = new AccountDashboardDisplayItem();
            displayItem.AccountName = accountName;
            displayItem.Total = total;
            return displayItem;
        }
    }
}
