using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;

namespace MyMoney.Tests.ReportsTests;

[TestClass]
public class BudgetReportCalculatorTests
{
    private DatabaseManager _databaseManager = null!;

    [TestInitialize]
    public void Setup()
    {
        _databaseManager = new(new MemoryStream());
    }

    [TestCleanup]
    public void Cleanup()
    {
        _databaseManager.Dispose();
    }

    [TestMethod]
    public async Task CalculateBudgetReport_ExcludesInternalSavingsTransactionsFromActuals()
    {
        var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _databaseManager.WriteCollection(
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

        _databaseManager.WriteCollection(
            "Transactions",
            [
                CreateTransaction(month, 800m, "Income", "Salary", 1),
                CreateTransaction(month, 999m, "Income", "Salary", -1),
                CreateTransaction(month, -50m, "Food", "Groceries", 1),
                CreateTransaction(month, -500m, "Food", "Groceries", -1),
                CreateTransaction(month, -25m, "Savings", "Emergency Fund", 1),
                CreateTransaction(month, 100m, "Savings", "Emergency Fund", -1),
            ]
        );

        var report = await BudgetReportCalculator.CalculateBudgetReport(month, _databaseManager);

        Assert.AreEqual(800m, report.income.Single().Actual.Value);
        Assert.AreEqual(50m, report.expenses.Single().Actual.Value);
        Assert.AreEqual(25m, report.savings.Single().Spent.Value);
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
