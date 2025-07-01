using System.Collections.ObjectModel;
using Moq;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class EditExpenseItem
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
    private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService;
    private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService;
    private Mock<Core.Database.IDatabaseReader> _mockDatabaseReader;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
        _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
        _mockExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();
        _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();
        _mockDatabaseReader = new Mock<Core.Database.IDatabaseReader>();

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns(new List<Budget>());

        _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockExpenseGroupDialogService.Object,
            _mockSavingsCategoryDialogService.Object
        );
    }

    [TestMethod]
    public async Task EditExpenseItem_SuccessfulEdit_UpdatesExpenseItem()
    {
        // Arrange
        var testBudget = new Budget
        {
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>
            {
                new() { CategoryName = "Group 1", SubItems = [
                    new() { Category = "Original Category", Amount = new(100m)}
                    ], SelectedSubItemIndex = 0 }
            }
        };
        _viewModel.CurrentBudget = testBudget;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        var dialogViewModel = new BudgetCategoryDialogViewModel
        {
            BudgetCategory = "Updated Category",
            BudgetAmount = new Currency(200m)
        };

        _mockBudgetCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockBudgetCategoryDialogService
            .Setup(x => x.GetViewModel())
            .Returns(dialogViewModel);

        // Act
        await _viewModel.EditExpenseItemCommand.ExecuteAsync(_viewModel.CurrentBudget.BudgetExpenseItems[0]);

        // Assert
        Assert.AreEqual(1, _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems.Count);
        Assert.AreEqual("Updated Category", _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Category);
        Assert.AreEqual(200m, _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Amount.Value);
    }

    [TestMethod]
    public async Task EditExpenseItem_CancelledEdit_MaintainsOriginalValues()
    {
        // Arrange
        var testBudget = new Budget
        {
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>
            {
                new() { CategoryName = "Group", SubItems = [
                    new() {Category = "Original Category", Amount = new(100m) }
                    ], SelectedSubItemIndex = 0 }
            }
        };
        
        _viewModel.CurrentBudget = testBudget;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        _mockBudgetCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Secondary);

        // Act
        await _viewModel.EditExpenseItemCommand.ExecuteAsync(_viewModel.CurrentBudget.BudgetExpenseItems[0]);

        // Assert
        Assert.AreEqual("Original Category", _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Category);
        Assert.AreEqual(100m, _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Amount.Value);
    }

    [TestMethod]
    public async Task EditExpenseItem_EditingDisabled_MakesNoChanges()
    {
        // Arrange
        var testBudget = new Budget
        {
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>
            {
                new() { CategoryName = "Original Group", SubItems = [
                    new() {Category = "Original Category", Amount = new(100m) }
                    ] }
            }
        };
        _viewModel.CurrentBudget = testBudget;
        _viewModel.ExpenseItemsSelectedIndex = 0;
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.EditExpenseItemCommand.ExecuteAsync(_viewModel.CurrentBudget.BudgetExpenseItems[0]);

        // Assert
        Assert.AreEqual("Original Category", _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Category);
        Assert.AreEqual(100m, _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Amount.Value);
        _mockBudgetCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()), 
            Times.Never);
    }

    [TestMethod]
    public async Task EditExpenseItem_NullBudget_MakesNoChanges()
    {
        // Arrange
        _viewModel.CurrentBudget = null;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        // Act
        await _viewModel.EditExpenseItemCommand.ExecuteAsync(null);

        // Assert
        _mockBudgetCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()), 
            Times.Never);
    }
}