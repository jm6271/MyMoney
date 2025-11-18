using System.Collections.ObjectModel;
using Moq;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[TestClass]
public class AddExpenseGroupTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
    private Mock<INewExpenseGroupDialogService> _mockNewExpenseGroupDialogService;
    private Mock<Core.Database.IDatabaseManager> _mockDatabaseReader;
    private Mock<ISavingsCategoryDialogService> _savingsCategoryDialogService;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
        _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
        _mockNewExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();
        _savingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();
        _mockDatabaseReader = new Mock<Core.Database.IDatabaseManager>();

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());
    }

    [TestMethod]
    public async Task AddExpenseGroup_WhenSuccessful_AddsNewGroupToBudget()
    {
        // Arrange
        var viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockNewExpenseGroupDialogService.Object,
            _savingsCategoryDialogService.Object
        );

        viewModel.CurrentBudget = new Budget { BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>() };

        var dialogViewModel = new NewExpenseGroupDialogViewModel { GroupName = "Test Group" };
        _mockNewExpenseGroupDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);
        _mockNewExpenseGroupDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);

        // Act
        await viewModel.AddExpenseGroupCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, viewModel.CurrentBudget.BudgetExpenseItems);
        Assert.AreEqual("Test Group", viewModel.CurrentBudget.BudgetExpenseItems[0].CategoryName);
    }

    [TestMethod]
    public async Task AddExpenseGroup_WhenCurrentBudgetIsNull_DoesNothing()
    {
        // Arrange
        var viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockNewExpenseGroupDialogService.Object,
            _savingsCategoryDialogService.Object
        );

        // Act
        await viewModel.AddExpenseGroupCommand.ExecuteAsync(null);

        // Assert
        _mockNewExpenseGroupDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task AddExpenseGroup_WhenEditingDisabled_DoesNothing()
    {
        // Arrange
        var viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockNewExpenseGroupDialogService.Object,
            _savingsCategoryDialogService.Object
        );

        viewModel.CurrentBudget = new Budget();
        viewModel.IsEditingEnabled = false;

        // Act
        await viewModel.AddExpenseGroupCommand.ExecuteAsync(null);

        // Assert
        _mockNewExpenseGroupDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task AddExpenseGroup_GroupAlreadyExists_ShowMessage()
    {
        // Arrange
        var viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockNewExpenseGroupDialogService.Object,
            _savingsCategoryDialogService.Object
        );

        viewModel.CurrentBudget = new Budget { BudgetExpenseItems = [new() { CategoryName = "Test Group" }] };

        var dialogViewModel = new NewExpenseGroupDialogViewModel { GroupName = "Test Group" };
        _mockNewExpenseGroupDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);
        _mockNewExpenseGroupDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);

        // Act
        await viewModel.AddExpenseGroupCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, viewModel.CurrentBudget.BudgetExpenseItems);
        _mockMessageBoxService.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }
}
