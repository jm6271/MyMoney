using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using System.Collections.ObjectModel;
using Wpf.Ui;
using Moq;
using MyMoney.Core.Database;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class NewTransactionTests
{
    private Mock<IContentDialogService> _contentDialogServiceMock;
    private Mock<IDatabaseReader> _databaseReaderMock;
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
        _databaseReaderMock = new Mock<IDatabaseReader>();
        _newAccountDialogServiceMock = new Mock<INewAccountDialogService>();
        _transferDialogServiceMock = new Mock<ITransferDialogService>();
        _transactionDialogServiceMock = new Mock<ITransactionDialogService>();
        _renameAccountDialogService = new Mock<IRenameAccountDialogService>();
        _messageBoxServiceMock = new Mock<IMessageBoxService>();

        _databaseReaderMock.Setup(x => x.GetCollection<Account>("Accounts"))
            .Returns([]);

        _viewModel = new(
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
    public async Task CreateNewTransaction_NoAccountsExist_ReturnsEarly()
    {
        // Arrange
        Assert.AreEqual(0, _viewModel.Accounts.Count);

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        _transactionDialogServiceMock.Verify(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateNewTransaction_NoSelectedAccountButAccountsExist_SelectsFirstAccount()
    {
        // Arrange
        var account = new Account { AccountName = "Test Account", Total = new Currency(1000) };
        _viewModel.Accounts.Add(account);
        Assert.IsNull(_viewModel.SelectedAccount);

        _transactionDialogServiceMock.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _transactionDialogServiceMock.Setup(x => x.GetSelectedPayee())
            .Returns("Test Payee");
        _transactionDialogServiceMock.Setup(x => x.GetViewModel())
            .Returns(new NewTransactionDialogViewModel 
            { 
                NewTransactionAmount = new Currency(500),
                NewTransactionIsExpense = true,
                NewTransactionDate = DateTime.Today,
                NewTransactionCategory = new() { Name = "Test Category", Group = "Income" },
                NewTransactionMemo = "Test Memo"
            });

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(account, _viewModel.SelectedAccount);
        Assert.AreEqual(0, _viewModel.SelectedAccountIndex);
    }

    [TestMethod]
    public async Task CreateNewTransaction_DialogCancelled_NoTransactionAdded()
    {
        // Arrange
        var account = new Account { AccountName = "Test Account", Total = new Currency(1000) };
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        _viewModel.SelectedAccountIndex = 0;

        _transactionDialogServiceMock.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(ContentDialogResult.Secondary);

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(0, account.Transactions.Count);
        Assert.AreEqual(1000, account.Total.Value);
    }

    [TestMethod]
    public async Task CreateNewTransaction_ValidTransaction_AddsTransactionAndUpdatesBalance()
    {
        // Arrange
        var account = new Account { AccountName = "Test Account", Total = new Currency(1000) };
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        _viewModel.SelectedAccountIndex = 0;

        _transactionDialogServiceMock.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _transactionDialogServiceMock.Setup(x => x.GetViewModel())
            .Returns(new NewTransactionDialogViewModel 
            { 
                NewTransactionAmount = new Currency(500),
                NewTransactionIsExpense = true,
                NewTransactionDate = DateTime.Today,
                NewTransactionCategory = new() { Name = "Test Category", Group = "Income" },
                NewTransactionMemo = "Test Memo"
            });
        _transactionDialogServiceMock.Setup(x => x.GetSelectedPayee())
            .Returns("Test Payee");

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, account.Transactions.Count);
        Assert.AreEqual(500, account.Total.Value); // 1000 - 500
        
        var transaction = account.Transactions[0];
        Assert.AreEqual("Test Payee", transaction.Payee);
        Assert.AreEqual("Test Category", transaction.Category.Name);
        Assert.AreEqual("Income", transaction.Category.Group);
        Assert.AreEqual("Test Memo", transaction.Memo);
        Assert.AreEqual(-500, transaction.Amount.Value);
        Assert.AreEqual(DateTime.Today, transaction.Date);
    }

    [TestMethod]
    public async Task CreateNewTransaction_ValidIncome_AddsTransactionAndUpdatesBalance()
    {
        // Arrange
        var account = new Account { AccountName = "Test Account", Total = new Currency(1000) };
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        _viewModel.SelectedAccountIndex = 0;

        _transactionDialogServiceMock.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        _transactionDialogServiceMock.Setup(x => x.GetViewModel())
            .Returns(new NewTransactionDialogViewModel 
            { 
                NewTransactionAmount = new Currency(500),
                NewTransactionIsExpense = false,
                NewTransactionDate = DateTime.Today,
                NewTransactionCategory = new() { Name = "Income", Group = "Income" },
                NewTransactionMemo = "Salary"
            });
        _transactionDialogServiceMock.Setup(x => x.GetSelectedPayee())
            .Returns("Employer");

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.AreEqual(1, account.Transactions.Count);
        Assert.AreEqual(1500, account.Total.Value); // 1000 + 500
        
        var transaction = account.Transactions[0];
        Assert.AreEqual("Employer", transaction.Payee);
        Assert.AreEqual("Income", transaction.Category.Name);
        Assert.AreEqual("Income", transaction.Category.Group);
        Assert.AreEqual("Salary", transaction.Memo);
        Assert.AreEqual(500, transaction.Amount.Value);
        Assert.AreEqual(DateTime.Today, transaction.Date);
    }
}