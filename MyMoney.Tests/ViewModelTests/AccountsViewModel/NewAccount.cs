using MyMoney.Core.FS.Models;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Moq;
using MyMoney.Services.ContentDialogs;
using MyMoney.Core.Database;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class NewAccountTest
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseReader> _mockDatabaseService;
    private Mock<INewAccountDialogService> _mockNewAccountDialogService;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseService = new Mock<IDatabaseReader>();
        _mockNewAccountDialogService = new Mock<INewAccountDialogService>();
        
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
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, _mockNewAccountDialogService.Object);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, _viewModel.Accounts.Count);
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
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, _mockNewAccountDialogService.Object);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, _viewModel.Accounts.Count);
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
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, _mockNewAccountDialogService.Object);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.AreEqual(0, _viewModel.Accounts.Count);
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

        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, _mockNewAccountDialogService.Object);

        // Act - Create first account
        SetupMockDialogSuccess(firstAccount);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Create second account
        SetupMockDialogSuccess(secondAccount);
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.AreEqual(2, _viewModel.Accounts.Count);
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
        _viewModel = new(_mockContentDialogService.Object, _mockDatabaseService.Object, _mockNewAccountDialogService.Object);
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