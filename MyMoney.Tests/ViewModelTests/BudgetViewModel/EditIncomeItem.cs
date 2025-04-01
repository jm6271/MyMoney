using System.Collections.ObjectModel;
using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class EditIncomeItemTests
{
    private Mock<IContentDialogService> _mockContentDialogService = null!;
    private Mock<IDatabaseReader> _mockDatabaseReader = null!;
    private Mock<IMessageBoxService> _mockMessageBoxService = null!;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService = null!;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService = null!;
    private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService = null!;
    private ViewModels.Pages.BudgetViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseReader>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
        _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
        _mockExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();

        // Setup mock database with a test budget
        var testBudget = new Budget
        {
            BudgetDate = DateTime.Now,
            BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
            BudgetIncomeItems = new ObservableCollection<BudgetItem>
            {
                new() { Category = "Test Income", Amount = new Currency(1000m) }
            }
        };

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns([testBudget]);

        _viewModel = new ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockExpenseGroupDialogService.Object
        );
    }

    [TestMethod]
    public async Task EditIncomeItem_SuccessfulEdit_UpdatesItemAndTotals()
    {
        // Arrange
        _viewModel.IncomeItemsSelectedIndex = 0;
        var newCategory = "Updated Income";
        var newAmount = new Currency(2000m);

        var dialogViewModel = new BudgetCategoryDialogViewModel
        {
            BudgetCategory = newCategory,
            BudgetAmount = newAmount
        };

        _mockBudgetCategoryDialogService.Setup(x => x.GetViewModel())
            .Returns(dialogViewModel);

        _mockBudgetCategoryDialogService.Setup(x => x.ShowDialogAsync(
            It.IsAny<IContentDialogService>(),
            It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);

        // Act
        await _viewModel.EditIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(newCategory, _viewModel.CurrentBudget!.BudgetIncomeItems[0].Category);
        Assert.AreEqual(newAmount.Value, _viewModel.CurrentBudget.BudgetIncomeItems[0].Amount.Value);
        Assert.AreEqual(newAmount.Value, _viewModel.IncomeTotal.Value);
    }

    [TestMethod]
    public async Task EditIncomeItem_NullCurrentBudget_MakesNoChanges()
    {
        // Arrange
        _viewModel.CurrentBudget = null;

        // Act
        await _viewModel.EditIncomeItemCommand.ExecuteAsync(null);

        // Assert
        _mockBudgetCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task EditIncomeItem_EditingDisabled_MakesNoChanges()
    {
        // Arrange
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.EditIncomeItemCommand.ExecuteAsync(null);

        // Assert
        _mockBudgetCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task EditIncomeItem_DialogResultNotPrimary_MakesNoChanges()
    {
        // Arrange
        _viewModel.IncomeItemsSelectedIndex = 0;
        var originalCategory = _viewModel.CurrentBudget!.BudgetIncomeItems[0].Category;
        var originalAmount = _viewModel.CurrentBudget.BudgetIncomeItems[0].Amount;

        _mockBudgetCategoryDialogService.Setup(x => x.ShowDialogAsync(
            It.IsAny<IContentDialogService>(),
            It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Secondary);

        // Act
        await _viewModel.EditIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(originalCategory, _viewModel.CurrentBudget.BudgetIncomeItems[0].Category);
        Assert.AreEqual(originalAmount, _viewModel.CurrentBudget.BudgetIncomeItems[0].Amount);
    }
}
