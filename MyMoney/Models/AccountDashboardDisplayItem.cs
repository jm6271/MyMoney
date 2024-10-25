using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Models
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
