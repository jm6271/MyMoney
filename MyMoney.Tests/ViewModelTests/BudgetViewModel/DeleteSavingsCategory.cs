using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class DeleteSavingsCategoryTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseReader> _mockDatabaseReader;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
    private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
    private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService;
    private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService;
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
        _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns(new List<Budget>());

        _viewModel = new(
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
    public async Task DeleteSavingsCategory_WhenCurrentBudgetIsNull_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = null;

        // Act
        await _viewModel.DeleteSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockMessageBoxService.Verify(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteSavingsCategory_WhenEditingIsDisabled_ShouldReturnWithoutAction()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        _viewModel.IsEditingEnabled = false;

        // Act
        await _viewModel.DeleteSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockMessageBoxService.Verify(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteSavingsCategory_WhenUserCancels_ShouldNotDeleteCategory()
    {
        // Arrange
        var budget = new Budget();
        budget.BudgetSavingsCategories.Add(new BudgetSavingsCategory { CategoryName = "Test Savings", Id = 1 });
        _viewModel.CurrentBudget = budget;
        _viewModel.IsEditingEnabled = true;
        _viewModel.SavingsCategoriesSelectedIndex = 0;
        _mockMessageBoxService.Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);

        // Act
        await _viewModel.DeleteSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, _viewModel.CurrentBudget.BudgetSavingsCategories.Count);
    }

    [TestMethod]
    public async Task DeleteSavingsCategory_WhenUserConfirms_ShouldDeleteCategoryAndReorderIds()
    {
        // Arrange
        var budget = new Budget();
        budget.BudgetSavingsCategories.Add(new BudgetSavingsCategory { CategoryName = "A", Id = 1 });
        budget.BudgetSavingsCategories.Add(new BudgetSavingsCategory { CategoryName = "B", Id = 2 });
        budget.BudgetSavingsCategories.Add(new BudgetSavingsCategory { CategoryName = "C", Id = 3 });
        _viewModel.CurrentBudget = budget;
        _viewModel.IsEditingEnabled = true;
        _viewModel.SavingsCategoriesSelectedIndex = 1; // Delete "B"
        _mockMessageBoxService.Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        await _viewModel.DeleteSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(2, _viewModel.CurrentBudget.BudgetSavingsCategories.Count);
        Assert.AreEqual("A", _viewModel.CurrentBudget.BudgetSavingsCategories[0].CategoryName);
        Assert.AreEqual(1, _viewModel.CurrentBudget.BudgetSavingsCategories[0].Id);
        Assert.AreEqual("C", _viewModel.CurrentBudget.BudgetSavingsCategories[1].CategoryName);
        Assert.AreEqual(2, _viewModel.CurrentBudget.BudgetSavingsCategories[1].Id);
    }
}