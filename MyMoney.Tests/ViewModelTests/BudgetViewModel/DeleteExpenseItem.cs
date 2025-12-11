using System.Collections.ObjectModel;
using Moq;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class DeleteExpenseItemTests
{
    private Mock<IContentDialogService> _contentDialogServiceMock;
    private Mock<IMessageBoxService> _messageBoxServiceMock;
    private Mock<Core.Database.IDatabaseManager> _databaseReaderMock;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _contentDialogServiceMock = new Mock<IContentDialogService>();
        _messageBoxServiceMock = new Mock<IMessageBoxService>();
        _databaseReaderMock = new Mock<Core.Database.IDatabaseManager>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        _databaseReaderMock.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

        _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _contentDialogServiceMock.Object,
            _databaseReaderMock.Object,
            _messageBoxServiceMock.Object,
            _mockContentDialogFactory.Object
        );
    }

    [TestMethod]
    public async Task DeleteExpenseItem_WhenCurrentBudgetIsNull_ShouldReturnEarly()
    {
        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(null);

        // Assert
        _messageBoxServiceMock.Verify(
            x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task DeleteExpenseItem_WhenEditingDisabled_ShouldReturnEarly()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetDate = DateTime.Now,
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>(),
        };
        _viewModel.CurrentBudget = budget;
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(null);

        // Assert
        _messageBoxServiceMock.Verify(
            x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
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
                new()
                {
                    CategoryName = "Test", /* Amount = new Currency(100m), Id = 1 */
                },
            },
        };
        _viewModel.CurrentBudget = budget;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        _messageBoxServiceMock
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);

        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(new BudgetExpenseCategory());

        // Assert
        Assert.HasCount(1, budget.BudgetExpenseItems);
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
                new()
                {
                    CategoryName = "Group 1",
                    SubItems =
                    [
                        new() { Category = "Category 1", Amount = new(50m) },
                        new() { Category = "Category 2", Amount = new(100m) },
                        new() { Category = "Category 3", Amount = new(200m) },
                    ],
                },
            },
        };
        _viewModel.CurrentBudget = budget;
        _viewModel.CurrentBudget.BudgetExpenseItems[0].SelectedSubItemIndex = 1; // Select "Category 2"

        _messageBoxServiceMock
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        await _viewModel.DeleteExpenseItemCommand.ExecuteAsync(budget.BudgetExpenseItems[0]);

        // Assert
        Assert.HasCount(1, budget.BudgetExpenseItems);
        Assert.HasCount(2, budget.BudgetExpenseItems[0].SubItems);
        Assert.AreEqual("Category 1", budget.BudgetExpenseItems[0].SubItems[0].Category);
        Assert.AreEqual("Category 3", budget.BudgetExpenseItems[0].SubItems[1].Category);
        Assert.AreEqual(1, budget.BudgetExpenseItems[0].SubItems[0].Id);
        Assert.AreEqual(2, budget.BudgetExpenseItems[0].SubItems[1].Id);
    }
}
