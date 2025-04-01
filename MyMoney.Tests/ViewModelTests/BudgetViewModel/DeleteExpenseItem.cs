using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using Moq;
using Wpf.Ui.Controls;
using Wpf.Ui;
using System.Collections.ObjectModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class DeleteExpenseItemTests
{
    private Mock<IContentDialogService> _contentDialogServiceMock;
    private Mock<IMessageBoxService> _messageBoxServiceMock;
    private Mock<INewBudgetDialogService> _newBudgetDialogServiceMock;
    private Mock<IBudgetCategoryDialogService> _budgetCategoryDialogServiceMock;
    private Mock<Core.Database.IDatabaseReader> _databaseReaderMock;
    private Mock<INewExpenseGroupDialogService> _expenseGroupDialogServiceMock;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _contentDialogServiceMock = new Mock<IContentDialogService>();
        _messageBoxServiceMock = new Mock<IMessageBoxService>();
        _newBudgetDialogServiceMock = new Mock<INewBudgetDialogService>();
        _budgetCategoryDialogServiceMock = new Mock<IBudgetCategoryDialogService>();
        _databaseReaderMock = new Mock<Core.Database.IDatabaseReader>();
        _expenseGroupDialogServiceMock = new Mock<INewExpenseGroupDialogService>();

        _databaseReaderMock.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns(new List<Budget>());

        _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _contentDialogServiceMock.Object,
            _databaseReaderMock.Object,
            _messageBoxServiceMock.Object,
            _newBudgetDialogServiceMock.Object,
            _budgetCategoryDialogServiceMock.Object,
            _expenseGroupDialogServiceMock.Object
        );
    }

    [TestMethod] 
    public async Task DeleteExpenseItem_WhenCurrentBudgetIsNull_ShouldReturnEarly()
    {
        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(null);

        // Assert
        _messageBoxServiceMock.Verify(x => x.ShowAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()), 
            Times.Never);
    }

    [TestMethod]
    public async Task DeleteExpenseItem_WhenEditingDisabled_ShouldReturnEarly()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetDate = DateTime.Now,
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>()
        };
        _viewModel.CurrentBudget = budget;
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(null);

        // Assert
        _messageBoxServiceMock.Verify(x => x.ShowAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task DeleteExpenseItem_WhenUserClicksNo_ShouldNotDelete()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetDate = DateTime.Now,
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>
            {
                new() { CategoryName = "Test", /* Amount = new Currency(100m), Id = 1 */}
            }
        };
        _viewModel.CurrentBudget = budget;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        _messageBoxServiceMock.Setup(x => x.ShowAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);

        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, budget.BudgetExpenseItems.Count);
    }

    [TestMethod]
    public async Task DeleteExpenseItem_WhenUserConfirms_ShouldDeleteAndReorderIds()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetDate = DateTime.Now,
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>
            {
                new() {CategoryName = "Item1"},
                new() {CategoryName = "Item2"},
                new() {CategoryName = "Item3"}
            }
        };
        _viewModel.CurrentBudget = budget;
        _viewModel.ExpenseItemsSelectedIndex = 1; // Delete middle item

        _messageBoxServiceMock.Setup(x => x.ShowAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(2, budget.BudgetExpenseItems.Count);
        Assert.AreEqual("Item1", budget.BudgetExpenseItems[0].CategoryName);
        Assert.AreEqual("Item3", budget.BudgetExpenseItems[1].CategoryName);
        Assert.AreEqual(1, budget.BudgetExpenseItems[0].Id);
        Assert.AreEqual(2, budget.BudgetExpenseItems[1].Id);
    }
}