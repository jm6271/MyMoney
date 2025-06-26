namespace MyMoney.Core.Models
{
    public class SavingsCategoryReportItem
    {
        public int Id { get; set; }

        public string Category { get; set; } = string.Empty;

        public Currency Saved { get; set; } = new(0m);

        public Currency Spent { get; set; } = new(0m);

        public Currency Balance { get; set; } = new(0m);
    }
}
