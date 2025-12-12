using System.Collections.ObjectModel;
using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class EditIncomeItemTests
{
    private Mock<IContentDialogService> _mockContentDialogService = null!;
    private Mock<IDatabaseManager> _mockDatabaseReader = null!;
    private Mock<IMessageBoxService> _mockMessageBoxService = null!;
    private Mock<IContentDialogFactory> _mockContentDialogFactory = null!;
    private ViewModels.Pages.BudgetViewModel _viewModel = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        // Setup mock database with a test budget
        var testBudget = new Budget
        {
            BudgetDate = DateTime.Now,
            BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
            BudgetIncomeItems = new ObservableCollection<BudgetItem>
            {
                new() { Category = "Test Income", Amount = new Currency(1000m) },
            },
        };

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns([testBudget]);

        _viewModel = new ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );

        await _viewModel.OnNavigatedToAsync();
        _viewModel.CurrentBudget = _viewModel.Budgets[0];
    }

    [TestMethod]
    public async Task EditIncomeItem_SuccessfulEdit_UpdatesItemAndTotals()
    {
        // Arrange
        _viewModel.IncomeItemsSelectedIndex = 0;
        var newCategory = "Updated Income";
        var newAmount = new Currency(2000m);

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>((ct) =>
            {
                var vm = fake.Object.DataContext as BudgetCategoryDialogViewModel;
                vm?.BudgetCategory = newCategory;
                vm?.BudgetAmount = newAmount;
            })
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<BudgetCategoryDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.EditIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(newCategory, _viewModel.CurrentBudget!.BudgetIncomeItems[0].Category);
        Assert.AreEqual(newAmount.Value, _viewModel.CurrentBudget.BudgetIncomeItems[0].Amount.Value);
        Assert.AreEqual(newAmount.Value, _viewModel.IncomeTotal.Value);
    }

    [TestMethod]
    public async Task EditIncomeItem_DialogResultNotPrimary_MakesNoChanges()
    {
        // Arrange
        _viewModel.IncomeItemsSelectedIndex = 0;
        var originalCategory = _viewModel.CurrentBudget!.BudgetIncomeItems[0].Category;
        var originalAmount = _viewModel.CurrentBudget.BudgetIncomeItems[0].Amount;

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>((ct) =>
            {
                var vm = fake.Object.DataContext as BudgetCategoryDialogViewModel;
                vm?.BudgetCategory = "Test Category";
                vm?.BudgetAmount = new Currency(150m);
            })
            .ReturnsAsync(ContentDialogResult.Secondary);

        _mockContentDialogFactory.Setup(x => x.Create<BudgetCategoryDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.EditIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(originalCategory, _viewModel.CurrentBudget.BudgetIncomeItems[0].Category);
        Assert.AreEqual(originalAmount, _viewModel.CurrentBudget.BudgetIncomeItems[0].Amount);
    }

    [TestMethod]
    public async Task EditIncomeItem_ItemWithSameNameExists_ShowMessage()
    {
        // Arrange
        _viewModel.IncomeItemsSelectedIndex = 0;
        _viewModel.CurrentBudget?.BudgetIncomeItems.Add(new() { Category = "Test Income 2", Amount = new(1000m) });
        var newCategory = "Test Income 2";
        var newAmount = new Currency(2000m);

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>((ct) =>
            {
                var vm = fake.Object.DataContext as BudgetCategoryDialogViewModel;
                vm?.BudgetCategory = newCategory;
                vm?.BudgetAmount = newAmount;
            })
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<BudgetCategoryDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.EditIncomeItemCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(2, _viewModel.CurrentBudget!.BudgetIncomeItems);
        _mockMessageBoxService.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }
}
