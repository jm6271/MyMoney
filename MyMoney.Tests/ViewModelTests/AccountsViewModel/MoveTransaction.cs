using GongSolutions.Wpf.DragDrop;
using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Exports;
using MyMoney.Core.Models;
using MyMoney.Helpers.DropHandlers;
using MyMoney.Services;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class MoveTransactionTests
{
    private DatabaseManager _databaseManager = null!;
    private Mock<IMessageBoxService> _messageBoxService = null!;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel = null!;
    private Account _sourceAccount = null!;
    private Account _destinationAccount = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _databaseManager = new(new MemoryStream());
        _messageBoxService = new();
        _messageBoxService
            .Setup(service => service.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Wpf.Ui.Controls.MessageBoxResult.None);

        _sourceAccount = new Account
        {
            Id = 1,
            AccountName = "Checking",
            Total = new Currency(150m),
        };
        _destinationAccount = new Account
        {
            Id = 2,
            AccountName = "Savings",
            Total = new Currency(20m),
        };

        _databaseManager.WriteCollection("Accounts", [_sourceAccount, _destinationAccount]);
        _viewModel = CreateViewModel();
        _viewModel.SelectedAccount = _viewModel.Accounts.Single(account => account.Id == 1);
        _viewModel.SelectedAccountIndex = 0;
        await _viewModel.LoadTransactions();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _databaseManager.Dispose();
    }

    [TestMethod]
    public async Task MoveTransactionAsync_IncomeMovesTransactionAndUpdatesBalances()
    {
        var transaction = AddTransaction(new Currency(50m));
        var source = _viewModel.Accounts.Single(account => account.Id == 1);
        var destination = _viewModel.Accounts.Single(account => account.Id == 2);
        var selectedAccount = _viewModel.SelectedAccount;

        var result = await _viewModel.MoveTransactionAsync(transaction, destination);

        Assert.IsTrue(result);
        Assert.AreEqual(100m, source.Total.Value);
        Assert.AreEqual(70m, destination.Total.Value);
        Assert.AreEqual(2, transaction.AccountId);
        Assert.IsFalse(_viewModel.SelectedAccountTransactions.Contains(transaction));
        Assert.AreSame(selectedAccount, _viewModel.SelectedAccount);
        Assert.IsNull(_viewModel.SelectedTransaction);
        Assert.AreEqual(-1, _viewModel.SelectedTransactionIndex);

        var persistedTransaction = _databaseManager
            .GetCollection<Transaction>("Transactions")
            .Single(item => item.Id == transaction.Id);
        Assert.AreEqual(2, persistedTransaction.AccountId);

        var persistedAccounts = _databaseManager.GetCollection<Account>("Accounts");
        Assert.AreEqual(100m, persistedAccounts.Single(account => account.Id == 1).Total.Value);
        Assert.AreEqual(70m, persistedAccounts.Single(account => account.Id == 2).Total.Value);
    }

    [TestMethod]
    public async Task MoveTransactionAsync_ExpenseUsesSignedBalanceMath()
    {
        var source = _viewModel.Accounts.Single(account => account.Id == 1);
        var destination = _viewModel.Accounts.Single(account => account.Id == 2);
        source.Total = new Currency(60m);
        destination.Total = new Currency(100m);
        var transaction = AddTransaction(new Currency(-40m));

        var result = await _viewModel.MoveTransactionAsync(transaction, destination);

        Assert.IsTrue(result);
        Assert.AreEqual(100m, source.Total.Value);
        Assert.AreEqual(60m, destination.Total.Value);
    }

    [TestMethod]
    public async Task MoveTransactionAsync_ExpenseExceedingDestinationBalanceIsRejected()
    {
        var source = _viewModel.Accounts.Single(account => account.Id == 1);
        var destination = _viewModel.Accounts.Single(account => account.Id == 2);
        source.Total = new Currency(60m);
        var transaction = AddTransaction(new Currency(-40m));

        var result = await _viewModel.MoveTransactionAsync(transaction, destination);

        Assert.IsFalse(result);
        Assert.AreEqual(60m, source.Total.Value);
        Assert.AreEqual(20m, destination.Total.Value);
        Assert.AreEqual(1, transaction.AccountId);
        Assert.IsTrue(_viewModel.SelectedAccountTransactions.Contains(transaction));
        _messageBoxService.Verify(
            service => service.ShowInfoAsync(
                "Insufficient Funds",
                It.Is<string>(message => message.Contains("destination account")),
                "OK"
            ),
            Times.Once
        );

        var persistedTransaction = _databaseManager
            .GetCollection<Transaction>("Transactions")
            .Single(item => item.Id == transaction.Id);
        Assert.AreEqual(1, persistedTransaction.AccountId);
    }

    [TestMethod]
    public async Task MoveTransactionAsync_IncomeExceedingSourceBalanceIsRejected()
    {
        var source = _viewModel.Accounts.Single(account => account.Id == 1);
        var destination = _viewModel.Accounts.Single(account => account.Id == 2);
        source.Total = new Currency(20m);
        var transaction = AddTransaction(new Currency(50m));

        var result = await _viewModel.MoveTransactionAsync(transaction, destination);

        Assert.IsFalse(result);
        Assert.AreEqual(20m, source.Total.Value);
        Assert.AreEqual(20m, destination.Total.Value);
        Assert.AreEqual(1, transaction.AccountId);
        Assert.IsTrue(_viewModel.SelectedAccountTransactions.Contains(transaction));
        _messageBoxService.Verify(
            service => service.ShowInfoAsync(
                "Insufficient Funds",
                It.Is<string>(message => message.Contains("source account")),
                "OK"
            ),
            Times.Once
        );

        var persistedTransaction = _databaseManager
            .GetCollection<Transaction>("Transactions")
            .Single(item => item.Id == transaction.Id);
        Assert.AreEqual(1, persistedTransaction.AccountId);
    }

    [TestMethod]
    public async Task MoveTransactionAsync_SameAccountIsRejected()
    {
        var source = _viewModel.Accounts.Single(account => account.Id == 1);
        var transaction = AddTransaction(new Currency(50m));

        var result = await _viewModel.MoveTransactionAsync(transaction, source);

        Assert.IsFalse(result);
        Assert.AreEqual(150m, source.Total.Value);
        Assert.AreEqual(1, transaction.AccountId);
        Assert.IsTrue(_viewModel.SelectedAccountTransactions.Contains(transaction));
    }

    [TestMethod]
    public void DragOver_ValidDestinationAllowsMoveAndHighlightsAccount()
    {
        var transaction = new Transaction(DateTime.Today, "Payee", new(), new Currency(50m), "")
        {
            AccountId = 1,
        };
        var dropInfo = CreateDropInfo(transaction, _viewModel.Accounts.Single(account => account.Id == 2));
        var handler = new TransactionAccountDropHandler(_viewModel);

        handler.DragOver(dropInfo.Object);

        Assert.AreEqual(DragDropEffects.Move, dropInfo.Object.Effects);
        Assert.AreEqual(DropTargetAdorners.Highlight, dropInfo.Object.DropTargetAdorner);
    }

    [TestMethod]
    public void DragOver_SameAccountInvalidDataOrInvalidTargetRejectsDrop()
    {
        var source = _viewModel.Accounts.Single(account => account.Id == 1);
        var transaction = new Transaction(DateTime.Today, "Payee", new(), new Currency(50m), "")
        {
            AccountId = source.Id,
        };
        var sameAccountDrop = CreateDropInfo(transaction, source);
        var invalidDrop = CreateDropInfo("not a transaction", _destinationAccount);
        var invalidTargetDrop = CreateDropInfo(transaction, "not an account");
        var handler = new TransactionAccountDropHandler(_viewModel);

        handler.DragOver(sameAccountDrop.Object);
        handler.DragOver(invalidDrop.Object);
        handler.DragOver(invalidTargetDrop.Object);

        Assert.AreEqual(DragDropEffects.None, sameAccountDrop.Object.Effects);
        Assert.AreEqual(DragDropEffects.None, invalidDrop.Object.Effects);
        Assert.AreEqual(DragDropEffects.None, invalidTargetDrop.Object.Effects);
    }

    private Transaction AddTransaction(Currency amount)
    {
        var transaction = new Transaction(DateTime.Today, "Payee", new(), amount, "")
        {
            Id = 10,
            AccountId = 1,
        };
        _databaseManager.WriteCollection("Transactions", [transaction]);
        _viewModel.SelectedAccountTransactions.Add(transaction);
        _viewModel.SelectedTransaction = transaction;
        _viewModel.SelectedTransactionIndex = 0;
        return transaction;
    }

    private static Mock<IDropInfo> CreateDropInfo(object data, object targetItem)
    {
        var dropInfo = new Mock<IDropInfo>();
        dropInfo.SetupGet(info => info.Data).Returns(data);
        dropInfo.SetupGet(info => info.TargetItem).Returns(targetItem);
        dropInfo.SetupProperty(info => info.Effects);
        dropInfo.SetupProperty(info => info.DropTargetAdorner);
        return dropInfo;
    }

    private MyMoney.ViewModels.Pages.AccountsViewModel CreateViewModel()
    {
        return new(
            Mock.Of<IContentDialogService>(),
            _databaseManager,
            _messageBoxService.Object,
            Mock.Of<IContentDialogFactory>(),
            Mock.Of<IFileDialogService>(),
            Mock.Of<ITransactionCsvExporter>()
        );
    }
}
