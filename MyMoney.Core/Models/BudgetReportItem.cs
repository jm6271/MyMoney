namespace MyMoney.Core.Models;

public class BudgetReportItem
{
    public int Id { get; set; }

    public string Group { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public Currency Budgeted { get; set; } = new(0m);

    public Currency Actual { get; set; } = new(0m);

    public Currency Remaining { get; set; } = new(0m);
}
