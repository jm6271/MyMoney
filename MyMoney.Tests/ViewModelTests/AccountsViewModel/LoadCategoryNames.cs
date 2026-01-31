using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.ViewModels.ContentDialogs; // Added missing namespace
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class LoadCategoryNames
{
    private Mock<IDatabaseManager> _databaseReader;
    private NewTransactionDialogViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _databaseReader = new Mock<IDatabaseManager>();

        // Setup empty accounts collection
        _databaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns(new List<Account>());

        // Initialize NewTransactionDialogViewModel
        _viewModel = new NewTransactionDialogViewModel(_databaseReader.Object);
    }

    [TestMethod]
    public void BudgetCategoryNames_WhenNoBudgetExists_IsEmpty()
    {
        // Arrange
        _databaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

        // Act
        var categoryNames = _viewModel.GetBudgetCategoryNames();

        // Assert
        Assert.IsFalse(categoryNames.Any());
    }

    [TestMethod]
    public void BudgetCategoryNames_WithBudgetItems_LoadsAllCategories()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetIncomeItems = [new BudgetItem { Category = "Salary" }, new BudgetItem { Category = "Bonus" }],
            BudgetExpenseItems =
            [
                new BudgetExpenseCategory
                {
                    CategoryName = "Category 1",
                    SubItems = [new() { Category = "Groceries" }, new() { Category = "Fast Food" }],
                },
                new BudgetExpenseCategory
                {
                    CategoryName = "Category 2",
                    SubItems = [new() { Category = "Utilities" }],
                },
            ],
            BudgetDate = DateTime.Now,
            BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
        };

        _databaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget> { budget });

        // Act
        var categoryNames = _viewModel.GetBudgetCategoryNames();

        // Assert
        Assert.HasCount(5, categoryNames);
        Assert.AreEqual("Salary", categoryNames[0].Name.ToString());
        Assert.AreEqual("Bonus", categoryNames[1].Name.ToString());
        Assert.AreEqual("Groceries", categoryNames[2].Name.ToString());
        Assert.AreEqual("Fast Food", categoryNames[3].Name.ToString());
        Assert.AreEqual("Utilities", categoryNames[4].Name.ToString());
    }

    [TestMethod]
    public void BudgetCategoryNames_WhenAccessedMultipleTimes_ClearsExistingCategories()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetIncomeItems = [new BudgetItem { Category = "Salary" }],
            BudgetExpenseItems =
            [
                new BudgetExpenseCategory
                {
                    CategoryName = "Category 1",
                    SubItems = [new() { Category = "Groceries" }],
                },
            ],
            BudgetDate = DateTime.Now,
            BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
        };

        _databaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget> { budget });

        // Act
        var firstAccess = _viewModel.GetBudgetCategoryNames();
        var secondAccess = _viewModel.GetBudgetCategoryNames();

        // Assert
        Assert.HasCount(2, secondAccess);
        Assert.AreEqual(1, secondAccess.Count(x => x.Name.ToString() == "Salary"));
        Assert.AreEqual(1, secondAccess.Count(x => x.Name.ToString() == "Groceries"));
    }
}

