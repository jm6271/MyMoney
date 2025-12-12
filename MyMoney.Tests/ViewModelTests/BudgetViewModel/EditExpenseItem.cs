using System.Collections.ObjectModel;
using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class EditExpenseItem
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private Mock<Core.Database.IDatabaseManager> _mockDatabaseReader;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();
        _mockDatabaseReader = new Mock<Core.Database.IDatabaseManager>();

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

        _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
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
                new()
                {
                    CategoryName = "Group 1",
                    SubItems = [new() { Category = "Original Category", Amount = new(100m) }],
                    SelectedSubItemIndex = 0,
                },
            },
        };
        _viewModel.CurrentBudget = testBudget;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as BudgetCategoryDialogViewModel;
                    vm?.BudgetCategory = "Updated Category";
                    vm?.BudgetAmount = new Currency(200m);
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<BudgetCategoryDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.EditExpenseItemCommand.ExecuteAsync(_viewModel.CurrentBudget.BudgetExpenseItems[0]);

        // Assert
        Assert.HasCount(1, _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems);
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
                new()
                {
                    CategoryName = "Group",
                    SubItems = [new() { Category = "Original Category", Amount = new(100m) }],
                    SelectedSubItemIndex = 0,
                },
            },
        };

        _viewModel.CurrentBudget = testBudget;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as BudgetCategoryDialogViewModel;
                    vm?.BudgetCategory = "Modified Category";
                    vm?.BudgetAmount = new Currency(300m);
                }
            )
            .ReturnsAsync(ContentDialogResult.Secondary);

        _mockContentDialogFactory.Setup(x => x.Create<BudgetCategoryDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.EditExpenseItemCommand.ExecuteAsync(_viewModel.CurrentBudget.BudgetExpenseItems[0]);

        // Assert
        Assert.AreEqual("Original Category", _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Category);
        Assert.AreEqual(100m, _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Amount.Value);
    }

    [TestMethod]
    public async Task EditExpenseItem_ItemNameAlreadyExists_ShowMessage()
    {
        // Arrange
        var testBudget = new Budget
        {
            BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>
            {
                new()
                {
                    CategoryName = "Group",
                    SubItems =
                    [
                        new() { Category = "Existing Category", Amount = new(100m) },
                        new() { Category = "Another Category", Amount = new(150m) },
                    ],
                    SelectedSubItemIndex = 0,
                },
            },
        };
        _viewModel.CurrentBudget = testBudget;
        _viewModel.ExpenseItemsSelectedIndex = 0;

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as BudgetCategoryDialogViewModel;
                    vm?.BudgetCategory = "Another Category";
                    vm?.BudgetAmount = new Currency(200m);
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<BudgetCategoryDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.EditExpenseItemCommand.ExecuteAsync(_viewModel.CurrentBudget.BudgetExpenseItems[0]);

        // Assert
        _mockMessageBoxService.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
        Assert.AreEqual("Existing Category", _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Category);
        Assert.AreEqual(100m, _viewModel.CurrentBudget.BudgetExpenseItems[0].SubItems[0].Amount.Value);
    }
}
