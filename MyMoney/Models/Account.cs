using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Models
{
    public partial class Account : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Transaction> _Transactions = [];

        [ObservableProperty]
        private string _AccountName = "";

        [ObservableProperty]
        private Currency _Total = new();

        public Account()
        {
            AccountName = string.Empty;
        }
    }
}
