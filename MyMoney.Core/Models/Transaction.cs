using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class Transaction : ObservableObject
    {
        [ObservableProperty]
        private DateTime _Date;

        public string DateFormatted
        {
            get
            {
                return Date.ToShortDateString();
            }
        }

        [ObservableProperty]
        private string _Payee;

        [ObservableProperty]
        private string _Category;

        [ObservableProperty]
        private Currency _Spend;

        [ObservableProperty]
        private Currency _Receive;

        [ObservableProperty]
        private Currency _Balance;

        [ObservableProperty]
        private string _Memo;

        public Transaction()
        {
            Date = DateTime.Today;

            Payee = string.Empty;
            Category = string.Empty;
            Memo = string.Empty;

            Spend = new();
            Receive = new();
            Balance = new();
        }

        public Transaction(DateTime Date, string Payee, string Category, Currency Spend, Currency Receive, Currency Balance, string Memo)
        {
            this.Date = Date;
            this.Payee = Payee;
            this.Category = Category;
            this.Spend = Spend;
            this.Receive = Receive;
            this.Memo = Memo;
            this.Balance = Balance;
        }
    }
}
