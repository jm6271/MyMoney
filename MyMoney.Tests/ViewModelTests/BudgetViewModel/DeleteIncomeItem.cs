// filepath: MyMoney/Tests/ViewModelTests/BudgetViewModel/DeleteIncomeItem.cs
using MyMoney.ViewModels.Pages;
using MyMoney.Core.Models;
using Moq;
using MyMoney.Services.ContentDialogs;
using MyMoney.Core.Database;
using Wpf.Ui.Controls;
using Wpf.Ui;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class DeleteIncomeItemTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
    private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService;
    private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService;
    private Mock<IDatabaseReader> _mockDatabaseReader;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;
    private Budget _testBudget;

    private int _originalNumberOfIncomeItems;

    [TestInitialize]
    public async Task Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
        _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
        _mockExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();
        _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseReader>();

        // Setup test budget with income items
        _testBudget = new Budget
        {
            BudgetDate = DateTime.Now,
            BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
            BudgetIncomeItems = 
            {
                new BudgetItem { Id = 1, Category = "Income 1", Amount = new Currency(1000m) },
                new BudgetItem { Id = 2, Category = "Income 2", Amount = new Currency(2000m) },
                new BudgetItem { Id = 3, Category = "Income 3", Amount = new Currency(3000m) }
            }
        };

        _originalNumberOfIncomeItems = _testBudget.BudgetIncomeItems.Count;

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns(new List<Budget> { _testBudget });

        _viewModel = new (
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockExpenseGroupDialogService.Object,
            _mockSavingsCategoryDialogService.Object
        );

        await _viewModel.OnNavigatedToAsync();
        _viewModel.CurrentBudget = _viewModel.Budgets[0];
    }

    [TestMethod]
    public async Task DeleteIncomeItem_WhenCurrentBudgetIsNull_DoesNothing()
    {
        // Arrange
        _viewModel.CurrentBudget = null;

        // Act
        await _viewModel.DeleteIncomeItemCommand.ExecuteAsync(null);

        // Assert
        _mockMessageBoxService.Verify(x => x.ShowAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteIncomeItem_WhenEditingDisabled_DoesNothing()
    {
        // Arrange
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.DeleteIncomeItemCommand.ExecuteAsync(null);

        // Assert
        _mockMessageBoxService.Verify(x => x.ShowAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteIncomeItem_WhenUserCancels_DoesNotDelete()
    {
        // Arrange
        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);
        
        var originalCount = _originalNumberOfIncomeItems;
        
        // Act
        await _viewModel.DeleteIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.CurrentBudget);
        Assert.AreEqual(originalCount, _viewModel.CurrentBudget.BudgetIncomeItems.Count);
    }

    [TestMethod]
    public async Task DeleteIncomeItem_WhenConfirmed_DeletesAndReindexes()
    {
        // Arrange
        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        _viewModel.IncomeItemsSelectedIndex = 1; // Select second item
        var expectedCount = _originalNumberOfIncomeItems - 1;

        // Act
        await _viewModel.DeleteIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.CurrentBudget);
        Assert.AreEqual(expectedCount, _viewModel.CurrentBudget.BudgetIncomeItems.Count);
        
        // Verify IDs are reindexed correctly
        for (int i = 0; i < _viewModel.CurrentBudget.BudgetIncomeItems.Count; i++)
        {
            Assert.AreEqual(i + 1, _viewModel.CurrentBudget.BudgetIncomeItems[i].Id);
        }
    }
}
