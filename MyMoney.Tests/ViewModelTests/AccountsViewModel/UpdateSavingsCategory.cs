using Moq;
using Wpf.Ui;
using MyMoney.Core.Database;
using MyMoney.Services.ContentDialogs;
using MyMoney.Core.Models;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


[TestClass]
public class UpdateSavingsCategoryTests
{
    private Mock<IContentDialogService> _contentDialogServiceMock;
    private Mock<IDatabaseManager> _databaseReaderMock;
    private Mock<INewAccountDialogService> _newAccountDialogServiceMock;
    private Mock<ITransferDialogService> _transferDialogServiceMock;
    private Mock<ITransactionDialogService> _transactionDialogServiceMock;
    private Mock<IRenameAccountDialogService> _renameAccountDialogService;
    private Mock<IMessageBoxService> _messageBoxServiceMock;
    private ViewModels.Pages.AccountsViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _contentDialogServiceMock = new Mock<IContentDialogService>();
        _databaseReaderMock = new Mock<IDatabaseManager>();
        _newAccountDialogServiceMock = new Mock<INewAccountDialogService>();
        _transferDialogServiceMock = new Mock<ITransferDialogService>();
        _transactionDialogServiceMock = new Mock<ITransactionDialogService>();
        _renameAccountDialogService = new Mock<IRenameAccountDialogService>();
        _messageBoxServiceMock = new Mock<IMessageBoxService>();

        _databaseReaderMock.Setup(x => x.GetCollection<Account>("Accounts")).Returns([]);

        _viewModel = new ViewModels.Pages.AccountsViewModel(
            _contentDialogServiceMock.Object,
            _databaseReaderMock.Object,
            _newAccountDialogServiceMock.Object,
            _transferDialogServiceMock.Object,
            _transactionDialogServiceMock.Object,
            _renameAccountDialogService.Object,
            _messageBoxServiceMock.Object
        );
    }


    [TestMethod]
    public async Task Test_UpdateSavingsCategory_NewTransaction_UpdatesSavingsCategory()
    {
        // Arrange
        _viewModel.Accounts.Add(new Account() { AccountName = "Test Account", Total = new(1000) });

        List<Budget> finalBudgetCollection = [];

        var budget = new Budget()
        {
            BudgetDate = DateTime.Today,
            BudgetTitle = DateTime.Today.ToString("MMMM, yyyy"),
            BudgetSavingsCategories = [
                new() {
                    CategoryName = "Test Category",
                    CurrentBalance = new(500),
                }
            ]
        };
        _databaseReaderMock.Setup(x => x.GetCollection<Budget>("Budgets")).Returns([budget]);

        _databaseReaderMock.Setup(x => x.WriteCollection("Budgets", It.IsAny<List<Budget>>()))
            .Callback<string, List<Budget>>((collectionName, collection) =>
            {
                finalBudgetCollection = collection;
            });

        _transactionDialogServiceMock.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(Wpf.Ui.Controls.ContentDialogResult.Primary);

        _transactionDialogServiceMock.Setup(x => x.GetViewModel())
            .Returns(new ViewModels.ContentDialogs.NewTransactionDialogViewModel()
            {
                NewTransactionAmount = new(100),
                NewTransactionCategory = new() { Group = "Savings", Name = "Test Category" },
                NewTransactionDate = DateTime.Today,
                NewTransactionIsExpense = true,
                NewTransactionIsIncome = false,
                NewTransactionPayee = "Test Payee",
            });

        _transactionDialogServiceMock.Setup(x => x.GetSelectedPayee())
            .Returns("Test Payee");

        _viewModel.SelectedAccountIndex = 0;
        _viewModel.SelectedAccount = _viewModel.Accounts[0];

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, finalBudgetCollection);
        Account account = _viewModel.Accounts[0];
        Assert.HasCount(1, account.Transactions);
        Assert.HasCount(1, finalBudgetCollection[0].BudgetSavingsCategories[0].Transactions);
        Assert.AreEqual(account.Transactions[0].TransactionHash, finalBudgetCollection[0].BudgetSavingsCategories[0].Transactions[0].TransactionHash);
        Assert.AreEqual(new(400), finalBudgetCollection[0].BudgetSavingsCategories[0].CurrentBalance);
    }

    [TestMethod]
    public async Task UpdateSavingsCategory_EditTransaction_SameCategoryName_UpdatesSavingsCategory()
    {
        // Arrange
        _viewModel.Accounts.Add(new Account() { AccountName = "Test Account", Total = new(1000) });

        List<Budget> finalBudgetCollection = [];

        var budget = new Budget()
        {
            BudgetDate = DateTime.Today,
            BudgetTitle = DateTime.Today.ToString("MMMM, yyyy"),
            BudgetSavingsCategories = [
                new() {
                    CategoryName = "Test Category",
                    CurrentBalance = new(500),
                }
            ]
        };

        // Add a transaction to edit
        Transaction originalTransaction = new(DateTime.Today, "Original Payee",
            new() { Group = "Savings", Name = "Test Category" }, new(-200), "Original Memo");
        _viewModel.Accounts[0].Transactions.Add(originalTransaction);
        budget.BudgetSavingsCategories[0].Transactions.Add(originalTransaction);

        _databaseReaderMock.Setup(x => x.GetCollection<Budget>("Budgets")).Returns([budget]);

        _databaseReaderMock.Setup(x => x.WriteCollection("Budgets", It.IsAny<List<Budget>>()))
            .Callback<string, List<Budget>>((collectionName, collection) =>
            {
                finalBudgetCollection = collection;
            });

        _transactionDialogServiceMock.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(Wpf.Ui.Controls.ContentDialogResult.Primary);

        _transactionDialogServiceMock.Setup(x => x.GetViewModel())
            .Returns(new ViewModels.ContentDialogs.NewTransactionDialogViewModel()
            {
                NewTransactionAmount = new(100),
                NewTransactionCategory = new() { Group = "Savings", Name = "Test Category" },
                NewTransactionDate = DateTime.Today,
                NewTransactionIsExpense = true,
                NewTransactionIsIncome = false,
                NewTransactionPayee = "New Payee",
                NewTransactionMemo = "New Memo"
            });

        _transactionDialogServiceMock.Setup(x => x.GetSelectedPayee())
            .Returns("New Payee");

        _viewModel.SelectedAccountIndex = 0;
        _viewModel.SelectedAccount = _viewModel.Accounts[0];
        _viewModel.SelectedTransactionIndex = 0;
        _viewModel.SelectedTransaction = _viewModel.Accounts[0].Transactions[0];

        // Act
        await _viewModel.EditTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, _viewModel.Accounts);
        var account = _viewModel.Accounts[0];
        Assert.HasCount(1, account.Transactions);
        Assert.HasCount(1, finalBudgetCollection);
        Assert.HasCount(1, finalBudgetCollection[0].BudgetSavingsCategories[0].Transactions);
        Assert.AreEqual(account.Transactions[0].TransactionHash, finalBudgetCollection[0].BudgetSavingsCategories[0].Transactions[0].TransactionHash);
        Assert.AreEqual(-100m, account.Transactions[0].Amount.Value);
        Assert.AreEqual(-100m, finalBudgetCollection[0].BudgetSavingsCategories[0].Transactions[0].Amount.Value);
        Assert.AreEqual(600m, finalBudgetCollection[0].BudgetSavingsCategories[0].CurrentBalance.Value);
    }

    [TestMethod]
    public async Task UpdateSavingsCategory_EditTransaction_DifferentCategoryName_UpdatesSavingsCategory()
    {
        // Arrange
        _viewModel.Accounts.Add(new Account() { AccountName = "Test Account", Total = new(1000) });

        List<Budget> finalBudgetCollection = [];

        var budget = new Budget()
        {
            BudgetDate = DateTime.Today,
            BudgetTitle = DateTime.Today.ToString("MMMM, yyyy"),
            BudgetSavingsCategories = [
                new() {
                    CategoryName = "Test Category",
                    CurrentBalance = new(400),
                },
                new() {
                    CategoryName = "Other Category",
                    CurrentBalance = new(1000),
                }
            ]
        };

        // Add a transaction to edit
        Transaction originalTransaction = new(DateTime.Today, "Original Payee",
            new() { Group = "Savings", Name = "Test Category" }, new(-100), "Original Memo");
        _viewModel.Accounts[0].Transactions.Add(originalTransaction);
        budget.BudgetSavingsCategories[0].Transactions.Add(originalTransaction);

        _databaseReaderMock.Setup(x => x.GetCollection<Budget>("Budgets")).Returns([budget]);

        _databaseReaderMock.Setup(x => x.WriteCollection("Budgets", It.IsAny<List<Budget>>()))
            .Callback<string, List<Budget>>((collectionName, collection) =>
            {
                finalBudgetCollection = collection;
            });

        _transactionDialogServiceMock.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(Wpf.Ui.Controls.ContentDialogResult.Primary);

        _transactionDialogServiceMock.Setup(x => x.GetViewModel())
            .Returns(new ViewModels.ContentDialogs.NewTransactionDialogViewModel()
            {
                NewTransactionAmount = new(100),
                NewTransactionCategory = new() { Group = "Savings", Name = "Other Category" },
                NewTransactionDate = DateTime.Today,
                NewTransactionIsExpense = true,
                NewTransactionIsIncome = false,
                NewTransactionPayee = "New Payee",
                NewTransactionMemo = "New Memo"
            });

        _transactionDialogServiceMock.Setup(x => x.GetSelectedPayee())
            .Returns("New Payee");

        _viewModel.SelectedAccountIndex = 0;
        _viewModel.SelectedAccount = _viewModel.Accounts[0];
        _viewModel.SelectedTransactionIndex = 0;
        _viewModel.SelectedTransaction = _viewModel.Accounts[0].Transactions[0];

        // Act
        await _viewModel.EditTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, _viewModel.Accounts);
        var account = _viewModel.Accounts[0];
        Assert.HasCount(1, account.Transactions);
        Assert.HasCount(1, finalBudgetCollection);
        Assert.HasCount(0, finalBudgetCollection[0].BudgetSavingsCategories[0].Transactions);
        Assert.HasCount(1, finalBudgetCollection[0].BudgetSavingsCategories[1].Transactions);
        Assert.AreEqual(account.Transactions[0].TransactionHash, finalBudgetCollection[0].BudgetSavingsCategories[1].Transactions[0].TransactionHash);
        Assert.AreEqual(-100m, account.Transactions[0].Amount.Value);
        Assert.AreEqual(-100m, finalBudgetCollection[0].BudgetSavingsCategories[1].Transactions[0].Amount.Value);
        Assert.AreEqual(500m, finalBudgetCollection[0].BudgetSavingsCategories[0].CurrentBalance.Value);
        Assert.AreEqual(900m, finalBudgetCollection[0].BudgetSavingsCategories[1].CurrentBalance.Value);
    }
}   
