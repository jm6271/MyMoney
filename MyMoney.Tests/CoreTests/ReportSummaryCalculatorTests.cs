using MyMoney.Core.Models;
using MyMoney.Core.Services.Reports;
using MyMoney.Core.Database;

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

    [TestMethod]
    public async Task DashboardTotals_IncludeAccountBackedSavingsTransactions()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        databaseManager.WriteCollection(
            "Budgets",
            [
                new Budget
                {
                    BudgetDate = month,
                    BudgetTitle = month.ToString("MMMM yyyy"),
                    BudgetIncomeItems = [new BudgetItem { Category = "Salary", Amount = new Currency(1000m) }],
                    BudgetExpenseItems =
                    [
                        new BudgetExpenseCategory
                        {
                            CategoryName = "Food",
                            SubItems = [new BudgetItem { Category = "Groceries", Amount = new Currency(300m) }],
                        },
                    ],
                    BudgetSavingsCategories =
                    [
                        new BudgetSavingsCategory
                        {
                            CategoryName = "Emergency Fund",
                            BudgetedAmount = new Currency(100m),
                            CurrentBalance = new Currency(100m),
                        },
                    ],
                },
            ]
        );
        databaseManager.WriteCollection(
            "Transactions",
            [
                CreateTransaction(month, 800m, "Income", "Salary", 1),
                CreateTransaction(month, -50m, "Food", "Groceries", 1),
                CreateTransaction(month, -25m, "Savings", "Emergency Fund", 1),
                CreateTransaction(month, 100m, "Savings", "Emergency Fund", -1),
            ]
        );
        var calculator = new ReportSummaryCalculator(databaseManager);

        var spending = await calculator.GetSpendingTotal(month);
        var cashFlow = await calculator.GetCashFlowTotal(month);

        Assert.AreEqual(75m, spending.Value);
        Assert.AreEqual(725m, cashFlow.Value);
    }

    private static Transaction CreateTransaction(
        DateTime date,
        decimal amount,
        string categoryGroup,
        string categoryName,
        int accountId
    )
    {
        return new Transaction(
            date,
            "Payee",
            new Category { Group = categoryGroup, Name = categoryName },
            new Currency(amount),
            ""
        )
        {
            AccountId = accountId,
        };
    }
}
