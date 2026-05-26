using MyMoney.Core.Models;
using MyMoney.Core.Services.Reports;

namespace MyMoney.Tests.CoreTests;

[TestClass]
public class ReportSummaryCalculatorTests
{
    [TestMethod]
    public async Task CalculateReportTotals_ReturnsIncomeExpenseAndReportTotals()
    {
        var calculator = new ReportSummaryCalculator(new MyMoney.Core.Database.DatabaseManager(new MemoryStream()));

        var result = await calculator.CalculateReportTotals(
            [
                new BudgetReportItem
                {
                    Actual = new Currency(100m),
                    Budgeted = new Currency(120m),
                    Remaining = new Currency(20m),
                },
            ],
            [
                new BudgetReportItem
                {
                    Actual = new Currency(40m),
                    Budgeted = new Currency(50m),
                    Remaining = new Currency(10m),
                },
            ]
        );

        Assert.AreEqual("Total", result.Income.Category);
        Assert.AreEqual("Total", result.Expenses.Category);
        Assert.AreEqual(100m, result.Income.Actual.Value);
        Assert.AreEqual(40m, result.Expenses.Actual.Value);
        Assert.AreEqual(60m, result.Total.Value);
    }
}
