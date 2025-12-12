using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
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
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        // Setup database reader to return empty collection
        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
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

        var fakeDialog = new Mock<IContentDialog>();
        fakeDialog.SetupAllProperties();
        fakeDialog.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Secondary);

        _mockContentDialogFactory.Setup(x => x.Create<SavingsCategoryDialog>()).Returns(fakeDialog.Object);

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

        var fakeDialog = new Mock<IContentDialog>();
        fakeDialog.SetupAllProperties();
        fakeDialog
            .Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fakeDialog.Object.DataContext as SavingsCategoryDialogViewModel;
                    vm!.Category = "Savings 2";
                    vm.Planned = new Currency(100m);
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<SavingsCategoryDialog>()).Returns(fakeDialog.Object);

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

        var fakeDialog = new Mock<IContentDialog>();
        fakeDialog.SetupAllProperties();
        fakeDialog
            .Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fakeDialog.Object.DataContext as SavingsCategoryDialogViewModel;
                    vm!.Category = "Test Savings";
                    vm.Planned = new Currency(100m);
                    vm.CurrentBalance = new Currency(600m);
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<SavingsCategoryDialog>()).Returns(fakeDialog.Object);

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

        var fakeDialog = new Mock<IContentDialog>();
        fakeDialog.SetupAllProperties();
        fakeDialog
            .Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fakeDialog.Object.DataContext as SavingsCategoryDialogViewModel;
                    vm!.Category = "Test Savings";
                    vm.Planned = new Currency(200m);
                    vm.CurrentBalance = new Currency(500m);
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<SavingsCategoryDialog>()).Returns(fakeDialog.Object);

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
