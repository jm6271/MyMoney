using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class EditSavingsCategoryTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseReader;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
    private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService;
    private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
        _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
        _mockExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();
        _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        // Setup database reader to return empty collection
        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockExpenseGroupDialogService.Object,
            _mockSavingsCategoryDialogService.Object,
            _mockContentDialogFactory.Object
        );
    }

    [TestMethod]
    public async Task EditSavingsCategory_WhenCurrentBudgetIsNull_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = null;

        // Act
        await _viewModel.EditSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockSavingsCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task EditSavingsCategory_WhenEditingIsDisabled_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.EditSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockSavingsCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task EditSavingsCategory_WhenDialogCancelled_ShouldNotModifyCategory()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var originalCategory = new BudgetSavingsCategory
        {
            CategoryName = "Test Savings",
            BudgetedAmount = new Currency(100m),
            CurrentBalance = new Currency(500m),
        };
        _viewModel.CurrentBudget.BudgetSavingsCategories.Add(originalCategory);
        _viewModel.SavingsCategoriesSelectedIndex = 0;

        _mockSavingsCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Secondary);

        // Act
        await _viewModel.EditSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual("Test Savings", _viewModel.CurrentBudget.BudgetSavingsCategories[0].CategoryName);
        Assert.AreEqual(100m, _viewModel.CurrentBudget.BudgetSavingsCategories[0].BudgetedAmount.Value);
        Assert.AreEqual(500m, _viewModel.CurrentBudget.BudgetSavingsCategories[0].CurrentBalance.Value);
    }

    [TestMethod]
    public async Task EditSavingsCategory_WhenCategoryNameAlreadyExists_ShouldShowErrorMessage()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        _viewModel.CurrentBudget.BudgetSavingsCategories.Add(
            new BudgetSavingsCategory { CategoryName = "Savings 1", BudgetedAmount = new Currency(100m) }
        );
        _viewModel.CurrentBudget.BudgetSavingsCategories.Add(
            new BudgetSavingsCategory { CategoryName = "Savings 2", BudgetedAmount = new Currency(200m) }
        );
        _viewModel.SavingsCategoriesSelectedIndex = 0;

        var dialogViewModel = new SavingsCategoryDialogViewModel
        {
            Category = "Savings 2", // Try to rename to existing name
            Planned = new Currency(100m),
        };

        _mockSavingsCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _mockSavingsCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.EditSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockMessageBoxService.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
        Assert.AreEqual("Savings 1", _viewModel.CurrentBudget.BudgetSavingsCategories[0].CategoryName);
    }

    [TestMethod]
    public async Task EditSavingsCategory_WhenBalanceUpdated_ShouldAddBalanceUpdateTransaction()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var originalCategory = new BudgetSavingsCategory
        {
            CategoryName = "Test Savings",
            BudgetedAmount = new Currency(100m),
            CurrentBalance = new Currency(500m),
        };
        _viewModel.CurrentBudget.BudgetSavingsCategories.Add(originalCategory);
        _viewModel.SavingsCategoriesSelectedIndex = 0;

        var dialogViewModel = new SavingsCategoryDialogViewModel
        {
            Category = "Test Savings",
            Planned = new Currency(100m),
            CurrentBalance = new Currency(600m), // Changed balance
        };

        _mockSavingsCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _mockSavingsCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.EditSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(600m, _viewModel.CurrentBudget.BudgetSavingsCategories[0].CurrentBalance.Value);
        Assert.IsTrue(
            _viewModel
                .CurrentBudget.BudgetSavingsCategories[0]
                .Transactions.Any(t => t.TransactionDetail == "Updated balance" && t.Amount.Value == 100m)
        ); // Should add transaction for 100m difference
    }

    [TestMethod]
    public async Task EditSavingsCategory_WhenPlannedAmountUpdated_ShouldUpdatePlannedTransaction()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var originalCategory = new BudgetSavingsCategory
        {
            CategoryName = "Test Savings",
            BudgetedAmount = new Currency(100m),
            CurrentBalance = new Currency(500m),
        };

        // Add initial planned transaction
        var plannedTransaction = new Transaction(
            DateTime.Today,
            "",
            new Category { Group = "Savings", Name = "Test Savings" },
            new Currency(100m),
            "Planned This Month"
        );
        originalCategory.Transactions.Add(plannedTransaction);
        originalCategory.PlannedTransactionHash = plannedTransaction.TransactionHash;

        _viewModel.CurrentBudget.BudgetSavingsCategories.Add(originalCategory);
        _viewModel.SavingsCategoriesSelectedIndex = 0;

        var dialogViewModel = new SavingsCategoryDialogViewModel
        {
            Category = "Test Savings",
            Planned = new Currency(200m), // Changed planned amount
            CurrentBalance = new Currency(500m),
        };

        _mockSavingsCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _mockSavingsCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.EditSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(200m, _viewModel.CurrentBudget.BudgetSavingsCategories[0].BudgetedAmount.Value);
        Assert.AreEqual(600m, _viewModel.CurrentBudget.BudgetSavingsCategories[0].CurrentBalance.Value); // 500 + 100
        var updatedPlannedTransaction = _viewModel
            .CurrentBudget.BudgetSavingsCategories[0]
            .Transactions.First(t => t.TransactionHash == originalCategory.PlannedTransactionHash);
        Assert.AreEqual(200m, updatedPlannedTransaction.Amount.Value);
    }
}
