using System.Collections.ObjectModel;
using LiteDB;
using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Tests;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class NewTransactionTests
{
    private Mock<IContentDialogService> _contentDialogServiceMock;
    private Mock<IMessageBoxService> _messageBoxServiceMock;
    private Mock<IContentDialogFactory> _contentDialogFactoryMock;
    private ViewModels.Pages.AccountsViewModel _viewModel;

    private DatabaseManager _databaseManager;

    [TestInitialize]
    public async Task Setup()
    {
        _contentDialogServiceMock = new Mock<IContentDialogService>();
        _messageBoxServiceMock = new Mock<IMessageBoxService>();
        _contentDialogFactoryMock = new Mock<IContentDialogFactory>();

        _databaseManager = new(":memory:");

        _databaseManager.WriteCollection("Accounts", [
            new Account { AccountName = "Test Account", Total = new Currency(1000m), Id = 1 },
        ]);

        _viewModel = new(
            _contentDialogServiceMock.Object,
            _databaseManager,
            _messageBoxServiceMock.Object,
            _contentDialogFactoryMock.Object
        );

        await _viewModel.OnNavigatedToAsync();
    }

    [TestMethod]
    public async Task CreateNewTransaction_DialogCancelled_NoTransactionAdded()
    {
        // Arrange
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Secondary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.IsEmpty(_viewModel.SelectedAccountTransactions);
        Assert.AreEqual(1000, _viewModel.Accounts[0].Total.Value);
    }

    [TestMethod]
    public async Task CreateNewTransaction_ValidTransaction_AddsTransactionAndUpdatesBalance()
    {
        // Arrange
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewTransactionDialogViewModel;
                    vm?.NewTransactionAmount = new Currency(500);
                    vm?.NewTransactionIsExpense = true;
                    vm?.NewTransactionDate = DateTime.Today;
                    vm?.NewTransactionCategory = new() { Name = "Test Category", Group = "Income" };
                    vm?.NewTransactionMemo = "Test Memo";
                    vm?.NewTransactionPayee = "Test Payee";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
        Assert.AreEqual(500, _viewModel.Accounts[0].Total.Value); // 1000 - 500

        var transaction = _viewModel.SelectedAccountTransactions[0];
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
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewTransactionDialogViewModel;
                    vm?.NewTransactionAmount = new Currency(500);
                    vm?.NewTransactionIsExpense = false;
                    vm?.NewTransactionDate = DateTime.Today;
                    vm?.NewTransactionCategory = new() { Name = "Income", Group = "Income" };
                    vm?.NewTransactionMemo = "Salary";
                    vm?.NewTransactionPayee = "Employer";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
        Assert.AreEqual(1500, _viewModel.Accounts[0].Total.Value); // 1000 + 500

        var transaction = _viewModel.SelectedAccountTransactions[0];
        Assert.AreEqual("Employer", transaction.Payee);
        Assert.AreEqual("Income", transaction.Category.Name);
        Assert.AreEqual("Income", transaction.Category.Group);
        Assert.AreEqual("Salary", transaction.Memo);
        Assert.AreEqual(500, transaction.Amount.Value);
        Assert.AreEqual(DateTime.Today, transaction.Date);
    }

    [TestMethod]
    public async Task CreateNewTransaction_AmountMoreThanAccountBalance_DoesNotCreateTransaction()
    {
        // Arrange
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewTransactionDialogViewModel;
                    vm?.NewTransactionAmount = new Currency(-2000);
                    vm?.NewTransactionIsExpense = true;
                    vm?.NewTransactionDate = DateTime.Today;
                    vm?.NewTransactionCategory = new() { Name = "Expense 1", Group = "Expense" };
                    vm?.NewTransactionMemo = "";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.IsEmpty(_viewModel.SelectedAccountTransactions);
        Assert.AreEqual(1000, _viewModel.Accounts[0].Total.Value);

        // Verify message box was shown for insufficient funds
        _messageBoxServiceMock.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }
}
