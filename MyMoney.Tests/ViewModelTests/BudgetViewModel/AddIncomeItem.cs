using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class AddIncomeItemTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseReader> _mockDatabaseReader;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
    private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseReader>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
        _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
        _mockExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();

        // Setup database reader to return empty collection
        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns(new List<Budget>());

        _viewModel = new (
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockExpenseGroupDialogService.Object
        );
    }

    [TestMethod]
    public async Task AddIncomeItem_WhenCurrentBudgetIsNull_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = null;

        // Act
        await _viewModel.AddIncomeItemCommand.ExecuteAsync(null);

        // Assert
        _mockBudgetCategoryDialogService.Verify(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), 
            It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task AddIncomeItem_WhenEditingIsDisabled_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.AddIncomeItemCommand.ExecuteAsync(null);

        // Assert
        _mockBudgetCategoryDialogService.Verify(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), 
            It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task AddIncomeItem_WhenDialogCancelled_ShouldNotAddItem()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var dialogViewModel = new BudgetCategoryDialogViewModel();

        _mockBudgetCategoryDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), 
            It.IsAny<string>())).ReturnsAsync(ContentDialogResult.Secondary);
        _mockBudgetCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.AddIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(0, _viewModel.CurrentBudget.BudgetIncomeItems.Count);
    }

    [TestMethod]
    public async Task AddIncomeItem_WhenDialogConfirmed_ShouldAddNewItem()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var dialogViewModel = new BudgetCategoryDialogViewModel
        {
            BudgetCategory = "Test Category",
            BudgetAmount = new Currency(100m)
        };

        _mockBudgetCategoryDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), 
            It.IsAny<string>())).ReturnsAsync(ContentDialogResult.Primary);
        _mockBudgetCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.AddIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, _viewModel.CurrentBudget.BudgetIncomeItems.Count);
        Assert.AreEqual("Test Category", _viewModel.CurrentBudget.BudgetIncomeItems[0].Category);
        Assert.AreEqual(100m, _viewModel.CurrentBudget.BudgetIncomeItems[0].Amount.Value);
    }
}
