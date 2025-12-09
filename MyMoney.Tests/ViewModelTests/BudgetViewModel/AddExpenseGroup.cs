using System.Collections.ObjectModel;
using Moq;
using MyMoney.Core.Models;
using MyMoney.Services;
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
    private Mock<IContentDialogFactory> _mockContentDialogFactory;

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
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());
    }

    [STATestMethod]
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
            _savingsCategoryDialogService.Object,
            _mockContentDialogFactory.Object
        );

        viewModel.CurrentBudget = new Budget { BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>() };

        _mockContentDialogService.Setup(s => s.ShowAsync(It.IsAny<ContentDialog>(), It.IsAny<CancellationToken>()))
            .Callback<ContentDialog, CancellationToken>((dlg, ct) =>
            {
                var vm = dlg.DataContext as NewExpenseGroupDialogViewModel;
                vm?.GroupName = "Test Group";
            })
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<ContentDialog>()).Returns(new ContentDialog());

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
            _savingsCategoryDialogService.Object,
            _mockContentDialogFactory.Object
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
            _savingsCategoryDialogService.Object,
            _mockContentDialogFactory.Object
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

    [STATestMethod]
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
            _savingsCategoryDialogService.Object,
            _mockContentDialogFactory.Object
        );

        viewModel.CurrentBudget = new Budget { BudgetExpenseItems = [new() { CategoryName = "Test Group" }] };

        _mockContentDialogService.Setup(s => s.ShowAsync(It.IsAny<ContentDialog>(), It.IsAny<CancellationToken>()))
            .Callback<ContentDialog, CancellationToken>((dlg, ct) =>
            {
                var vm = dlg.DataContext as NewExpenseGroupDialogViewModel;
                vm?.GroupName = "Test Group";
            })
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<ContentDialog>()).Returns(new ContentDialog());

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
