namespace MyMoney.Models
{
    public class Transaction
    {
        public DateTime Date { get; set; }

        public string DateFormatted
        {
            get
            {
                return Date.ToShortDateString();
            }
        }

        public string Payee { get; set; }

        public string Category { get; set; }

        public Currency Spend { get; set; }

        public Currency Receive { get; set; }

        public Currency Balance { get; set; }

        public string Memo { get; set; }

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
