using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;

namespace MyMoney.Tests.ReportsTests;

[TestClass]
public class IncomeVsExpensesCalculatorTests
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
    public async Task GetPastYearsTotals_ReturnsElevenChronologicalYearsWithZeroesForMissingData()
    {
        var currentYear = DateTime.Today.Year;
        _databaseManager.WriteCollection(
            "Transactions",
            [
                CreateTransaction(currentYear - 10, 125m, "Salary"),
                CreateTransaction(currentYear - 2, 300m, "Salary"),
                CreateTransaction(currentYear - 2, -80m, "Food"),
                CreateTransaction(currentYear, 500m, "Salary"),
                CreateTransaction(currentYear, -120m, "Food"),
            ]
        );

        var income = await IncomeVsExpensesCalculator.GetPastYearsIncome(_databaseManager);
        var expenses = await IncomeVsExpensesCalculator.GetPastYearsExpenses(_databaseManager);
        var labels = IncomeVsExpensesCalculator.GetYearNames();

        Assert.HasCount(11, income);
        Assert.HasCount(11, expenses);
        Assert.AreEqual((currentYear - 10).ToString(), labels[0]);
        Assert.AreEqual(currentYear.ToString(), labels[^1]);
        CollectionAssert.AreEqual(
            new List<double> { 125, 0, 0, 0, 0, 0, 0, 0, 300, 0, 500 },
            income
        );
        CollectionAssert.AreEqual(
            new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 80, 0, 120 },
            expenses
        );
    }

    [TestMethod]
    public async Task GetPastYearsTotals_ExcludesOutOfRangeAndUncategorizedTransactions()
    {
        var currentYear = DateTime.Today.Year;
        _databaseManager.WriteCollection(
            "Transactions",
            [
                CreateTransaction(currentYear - 11, 1000m, "Salary"),
                CreateTransaction(currentYear, 200m, ""),
                CreateTransaction(currentYear, -50m, "Food"),
            ]
        );

        var income = await IncomeVsExpensesCalculator.GetPastYearsIncome(_databaseManager);
        var expenses = await IncomeVsExpensesCalculator.GetPastYearsExpenses(_databaseManager);

        Assert.AreEqual(0, income.Sum());
        Assert.AreEqual(50, expenses.Sum());
    }

    [TestMethod]
    public async Task GetPastYearsTotals_ExcludesInternalSavingsTransactions()
    {
        var currentYear = DateTime.Today.Year;
        _databaseManager.WriteCollection(
            "Transactions",
            [
                CreateTransaction(currentYear, 200m, "Salary"),
                CreateTransaction(currentYear, -50m, "Food"),
                CreateTransaction(currentYear, 1000m, "Emergency Fund", -1),
                CreateTransaction(currentYear, -500m, "Emergency Fund", -1),
            ]
        );

        var income = await IncomeVsExpensesCalculator.GetPastYearsIncome(_databaseManager);
        var expenses = await IncomeVsExpensesCalculator.GetPastYearsExpenses(_databaseManager);

        Assert.AreEqual(200, income.Sum());
        Assert.AreEqual(50, expenses.Sum());
    }

    private static Transaction CreateTransaction(int year, decimal amount, string category, int accountId = 1)
    {
        return new Transaction(
            new DateTime(year, 6, 15),
            "Payee",
            new Category { Name = category },
            new Currency(amount),
            ""
        )
        {
            AccountId = accountId,
        };
    }
}
