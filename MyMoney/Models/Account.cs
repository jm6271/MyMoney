using System.Collections.ObjectModel;

namespace MyMoney.Models
{
    public partial class Account : ObservableObject
    {
        [ObservableProperty]
        private int _Id = 0;

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
