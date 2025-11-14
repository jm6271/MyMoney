using MyMoney.Core.Models;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Moq;
using MyMoney.Services.ContentDialogs;
using MyMoney.Core.Database;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[TestClass]
public class NewAccountTest
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseService;
    private Mock<INewAccountDialogService> _mockNewAccountDialogService;
    private Mock<ITransactionDialogService> _mockTransactionDialogService;
    private Mock<IRenameAccountDialogService> _mockRenameAccountDialogService;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;
    private Mock<ITransferDialogService> _mockTransferDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<IUpdateAccountBalanceDialogService> _mockUpdateAccountBalanceDialogService;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseService = new Mock<IDatabaseManager>();
        _mockNewAccountDialogService = new Mock<INewAccountDialogService>();
        _mockTransferDialogService = new Mock<ITransferDialogService>();
        _mockTransactionDialogService = new Mock<ITransactionDialogService>();
        _mockRenameAccountDialogService = new Mock<IRenameAccountDialogService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockUpdateAccountBalanceDialogService = new Mock<IUpdateAccountBalanceDialogService>();

        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts"))
            .Returns([]);
    }

    [TestMethod]
    public void CreateNewAccount_Success_CreatesAccountWithCorrectValues()
    {
        // Arrange
        var dialogViewModel = new NewAccountDialogViewModel
        {
            AccountName = "Checking",
            StartingBalance = new Currency(1000m)
        };

        SetupMockDialogSuccess(dialogViewModel);

        // Act
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, 
            _mockNewAccountDialogService.Object, _mockTransferDialogService.Object,
            _mockTransactionDialogService.Object, _mockRenameAccountDialogService.Object,
            _mockMessageBoxService.Object, _mockUpdateAccountBalanceDialogService.Object);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.HasCount(1, _viewModel.Accounts);
        Assert.AreEqual("Checking", _viewModel.Accounts[0].AccountName);
        Assert.AreEqual(1000m, _viewModel.Accounts[0].Total.Value);
        Assert.IsTrue(_viewModel.TransactionsEnabled);
    }

    [TestMethod]
    public void CreateNewAccount_WithZeroBalance_CreatesAccount()
    {
        // Arrange
        var dialogViewModel = new NewAccountDialogViewModel
        {
            AccountName = "Empty Account",
            StartingBalance = new Currency(0m)
        };

        SetupMockDialogSuccess(dialogViewModel);

        // Act
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, 
            _mockNewAccountDialogService.Object, _mockTransferDialogService.Object,
            _mockTransactionDialogService.Object, _mockRenameAccountDialogService.Object,
            _mockMessageBoxService.Object, _mockUpdateAccountBalanceDialogService.Object);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.HasCount(1, _viewModel.Accounts);
        Assert.AreEqual(0m, _viewModel.Accounts[0].Total.Value);
    }

    [TestMethod]
    public void CreateNewAccount_UserCancelsDialog_DoesNotCreateAccount()
    {
        // Arrange
        _mockNewAccountDialogService
            .Setup(service => service.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(ContentDialogResult.None);

        // Act
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, 
            _mockNewAccountDialogService.Object, _mockTransferDialogService.Object,
            _mockTransactionDialogService.Object, _mockRenameAccountDialogService.Object,
            _mockMessageBoxService.Object, _mockUpdateAccountBalanceDialogService.Object);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.IsEmpty(_viewModel.Accounts);
    }

    [TestMethod]
    public void CreateNewAccount_MultipleAccounts_AddsAllAccounts()
    {
        // Arrange
        var firstAccount = new NewAccountDialogViewModel
        {
            AccountName = "First Account",
            StartingBalance = new Currency(100m)
        };

        var secondAccount = new NewAccountDialogViewModel
        {
            AccountName = "Second Account", 
            StartingBalance = new Currency(200m)
        };

        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, 
            _mockNewAccountDialogService.Object , _mockTransferDialogService.Object,
            _mockTransactionDialogService.Object, _mockRenameAccountDialogService.Object,
            _mockMessageBoxService.Object, _mockUpdateAccountBalanceDialogService.Object);

        // Act - Create first account
        SetupMockDialogSuccess(firstAccount);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Create second account
        SetupMockDialogSuccess(secondAccount);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.HasCount(2, _viewModel.Accounts);
        Assert.AreEqual("First Account", _viewModel.Accounts[0].AccountName);
        Assert.AreEqual("Second Account", _viewModel.Accounts[1].AccountName);
        Assert.AreEqual(100m, _viewModel.Accounts[0].Total.Value);
        Assert.AreEqual(200m, _viewModel.Accounts[1].Total.Value);
    }

    [TestMethod]
    public void CreateNewAccount_FirstAccountCreated_EnablesTransactions()
    {
        // Arrange
        var dialogViewModel = new NewAccountDialogViewModel
        {
            AccountName = "Test Account",
            StartingBalance = new Currency(100m)
        };

        SetupMockDialogSuccess(dialogViewModel);

        // Act
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, 
            _mockNewAccountDialogService.Object, _mockTransferDialogService.Object,
            _mockTransactionDialogService.Object, _mockRenameAccountDialogService.Object,
            _mockMessageBoxService.Object, _mockUpdateAccountBalanceDialogService.Object);
        Assert.IsFalse(_viewModel.TransactionsEnabled); // Initially disabled
        
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.IsTrue(_viewModel.TransactionsEnabled);
    }

    private void SetupMockDialogSuccess(NewAccountDialogViewModel dialogViewModel)
    {
        _mockNewAccountDialogService
            .Setup(service => service.ShowDialogAsync(It.IsAny<IContentDialogService>()))
            .ReturnsAsync(ContentDialogResult.Primary);
        
        _mockNewAccountDialogService
            .Setup(service => service.GetViewModel())
            .Returns(dialogViewModel);
    }
}