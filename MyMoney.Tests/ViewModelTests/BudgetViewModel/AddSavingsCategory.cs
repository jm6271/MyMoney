using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class AddSavingsCategoryTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseReader;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
    private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService;
    private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
        _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
        _mockExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();
        _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        // Setup database reader to return empty collection
        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockNewBudgetDialogService.Object,
            _mockBudgetCategoryDialogService.Object,
            _mockExpenseGroupDialogService.Object,
            _mockSavingsCategoryDialogService.Object,
            _mockContentDialogFactory.Object
        );
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenCurrentBudgetIsNull_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = null;

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockSavingsCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenEditingIsDisabled_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockSavingsCategoryDialogService.Verify(
            x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenDialogCancelled_ShouldNotAddCategory()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var dialogViewModel = new SavingsCategoryDialogViewModel();

        _mockSavingsCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Secondary);
        _mockSavingsCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.IsEmpty(_viewModel.CurrentBudget.BudgetSavingsCategories);
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenDialogConfirmed_ShouldAddNewCategory()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var dialogViewModel = new SavingsCategoryDialogViewModel
        {
            Category = "Test Savings",
            Planned = new Currency(200m),
        };

        _mockSavingsCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _mockSavingsCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, _viewModel.CurrentBudget.BudgetSavingsCategories);
        Assert.AreEqual("Test Savings", _viewModel.CurrentBudget.BudgetSavingsCategories[0].CategoryName);
        Assert.AreEqual(200m, _viewModel.CurrentBudget.BudgetSavingsCategories[0].BudgetedAmount.Value);
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenSavingsCategoryAlreadyExists_ShouldShowErrorMessage()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        _viewModel.CurrentBudget.BudgetSavingsCategories.Add(
            new BudgetSavingsCategory { CategoryName = "Existing Savings", BudgetedAmount = new Currency(100m) }
        );

        var dialogViewModel = new SavingsCategoryDialogViewModel
        {
            Category = "Existing Savings",
            Planned = new Currency(200m),
        };

        _mockSavingsCategoryDialogService
            .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _mockSavingsCategoryDialogService.Setup(x => x.GetViewModel()).Returns(dialogViewModel);

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockMessageBoxService.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
        Assert.HasCount(1, _viewModel.CurrentBudget.BudgetSavingsCategories);
    }
}
