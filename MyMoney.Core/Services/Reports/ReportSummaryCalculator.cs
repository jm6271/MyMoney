using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;

namespace MyMoney.Core.Services.Reports;

public sealed class ReportSummaryCalculator
{
    private readonly IDatabaseManager _databaseManager;

    public ReportSummaryCalculator(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public static BudgetReportItem CalculateTotal(IEnumerable<BudgetReportItem> items)
    {
        var total = new BudgetReportItem();
        foreach (var item in items)
        {
            total.Actual += item.Actual;
            total.Budgeted += item.Budgeted;
            total.Remaining += item.Remaining;
        }

        return total;
    }

    public async Task<(BudgetReportItem Income, BudgetReportItem Expenses, Currency Total)> CalculateReportTotals(
        IList<BudgetReportItem> incomeItems,
        IList<BudgetReportItem> expenseItems
    )
    {
        var incomeTotal = await Task.Run(() => CalculateTotal(incomeItems));
        var expenseTotal = await Task.Run(() => CalculateTotal(expenseItems));

        incomeTotal.Category = "Total";
        expenseTotal.Category = "Total";

        return (incomeTotal, expenseTotal, incomeTotal.Actual - expenseTotal.Actual);
    }

    public async Task<Currency> GetCashFlowTotal(DateTime month)
    {
        var report = await BudgetReportCalculator.CalculateBudgetReport(month, _databaseManager);

        Currency income = new(0m);
        foreach (var item in report.income)
            income.Value += item.Actual.Value;

        Currency expense = new(0m);
        foreach (var item in report.expenses)
            expense.Value += item.Actual.Value;

        return income - expense;
    }

    public async Task<Currency> GetSpendingTotal(DateTime month)
    {
        var report = await BudgetReportCalculator.CalculateExpenseReportItems(month, _databaseManager);
        Currency totalSpending = new(0m);
        foreach (var item in report)
            totalSpending.Value += item.Actual.Value;

        return totalSpending;
    }
}
