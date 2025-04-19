using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using Wpf.Ui;
using MyMoney.Views.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class LoadCategoryNames
{
    private Mock<IContentDialogService> _contentDialogService;
    private Mock<IDatabaseReader> _databaseReader;
    private Mock<INewAccountDialogService> _newAccountDialogService;
    private Mock<ITransferDialogService> _transferDialogService;
    private Mock<ITransactionDialogService> _transactionDialogService;
    private Mock<IRenameAccountDialogService> _renameDialogService;
    private Mock<IMessageBoxService> _messageBoxService;
    private ViewModels.Pages.AccountsViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _contentDialogService = new Mock<IContentDialogService>();
        _databaseReader = new Mock<IDatabaseReader>();
        _newAccountDialogService = new Mock<INewAccountDialogService>();
        _transferDialogService = new Mock<ITransferDialogService>();
        _transactionDialogService = new Mock<ITransactionDialogService>();
        _renameDialogService = new Mock<IRenameAccountDialogService>();
        _messageBoxService = new Mock<IMessageBoxService>();
            
        // Setup empty accounts collection
        _databaseReader.Setup(x => x.GetCollection<Account>("Accounts"))
            .Returns(new List<Account>());
    }

    [TestMethod]
    public void LoadCategoryNames_WhenNoBudgetExists_CategoryNamesIsEmpty()
    {
        // Arrange
        _databaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns(new List<Budget>());

        // Act
        _viewModel = new ViewModels.Pages.AccountsViewModel(
            _contentDialogService.Object,
            _databaseReader.Object,
            _newAccountDialogService.Object,
            _transferDialogService.Object,
            _transactionDialogService.Object,
            _renameDialogService.Object,
            _messageBoxService.Object);

        // Assert
        Assert.AreEqual(0, _viewModel.CategoryNames.Count);
    }

    [TestMethod]
    public void LoadCategoryNames_WithBudgetItems_LoadsAllCategories()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetIncomeItems =
            [
                new BudgetItem { Category = "Salary" },
                new BudgetItem { Category = "Bonus" }
            ],
            BudgetExpenseItems =
            [
                new BudgetExpenseCategory { CategoryName = "Category 1", SubItems = [
                    new () { Category = "Groceries"},
                    new () { Category = "Fast Food"}
                    ] },
                new BudgetExpenseCategory { CategoryName = "Category 2", SubItems = [
                    new () {Category = "Utilities" }
                    ] }
            ],
            BudgetDate = DateTime.Now,
            BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
        };

        _databaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns([budget]);

        // Act
        _viewModel = new ViewModels.Pages.AccountsViewModel(
            _contentDialogService.Object,
            _databaseReader.Object,
            _newAccountDialogService.Object,
            _transferDialogService.Object,
            _transactionDialogService.Object,
            _renameDialogService.Object,
            _messageBoxService.Object);

        // Assert
        Assert.AreEqual(5, _viewModel.CategoryNames.Count);
        Assert.AreEqual("Salary", _viewModel.CategoryNames[0].Item.ToString());
        Assert.AreEqual("Bonus", _viewModel.CategoryNames[1].Item.ToString());
        Assert.AreEqual("Groceries", _viewModel.CategoryNames[2].Item.ToString());
        Assert.AreEqual("Fast Food", _viewModel.CategoryNames[3].Item.ToString());
        Assert.AreEqual("Utilities", _viewModel.CategoryNames[4].Item.ToString());  
    }

    [TestMethod]
    public void LoadCategoryNames_WhenCalledMultipleTimes_ClearsExistingCategories()
    {
        // Arrange
        var budget = new Budget
        {
            BudgetIncomeItems = [new BudgetItem { Category = "Salary" }],
            BudgetExpenseItems = [new BudgetExpenseCategory { CategoryName = "Category 1", SubItems = [ new() { Category = "Groceries"}] }],
            BudgetDate = DateTime.Now,
            BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
        };

        _databaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
            .Returns([budget]);

        _viewModel = new ViewModels.Pages.AccountsViewModel(
            _contentDialogService.Object,
            _databaseReader.Object,
            _newAccountDialogService.Object,
            _transferDialogService.Object,
            _transactionDialogService.Object,
            _renameDialogService.Object,
            _messageBoxService.Object);

        // Act
        _viewModel.OnPageNavigatedTo(); // This calls LoadCategoryNames again

        // Assert
        Assert.AreEqual(2, _viewModel.CategoryNames.Count);
        Assert.AreEqual(1, _viewModel.CategoryNames.Count(x => x.Item.ToString() == "Salary"));
        Assert.AreEqual(1, _viewModel.CategoryNames.Count(x => x.Item.ToString() == "Groceries"));
    }
}