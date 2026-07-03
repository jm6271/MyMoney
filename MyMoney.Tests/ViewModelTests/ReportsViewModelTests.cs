using MyMoney.Core.Database;
using MyMoney.Core.Reports;
using MyMoney.ViewModels.Pages;

namespace MyMoney.Tests.ViewModelTests;

[TestClass]
public class ReportsViewModelTests
{
    [TestMethod]
    public void IncomeVsExpensesPeriod_DefaultsToMonths()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        var viewModel = new ReportsViewModel(databaseManager);

        Assert.AreEqual(0, viewModel.IncomeVsExpensesPeriodIndex);
    }

    [TestMethod]
    public async Task IncomeVsExpensesPeriod_WhenYearsSelected_UpdatesSeriesAndYearLabels()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        var viewModel = new ReportsViewModel(databaseManager);

        viewModel.IncomeVsExpensesPeriodIndex = 1;

        var expectedLabels = IncomeVsExpensesCalculator.GetYearNames(11);
        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (viewModel.IncomeVsExpensesXAxes[0].Labels?.SequenceEqual(expectedLabels) == true)
                break;

            await Task.Delay(10);
        }

        Assert.HasCount(2, viewModel.IncomeVsExpensesSeries);
        CollectionAssert.AreEqual(
            expectedLabels,
            viewModel.IncomeVsExpensesXAxes[0].Labels?.ToList()
        );
    }
}
