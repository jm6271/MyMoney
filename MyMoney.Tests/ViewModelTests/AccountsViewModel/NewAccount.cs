using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[TestClass]
public class NewAccountTest
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseService = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts")).Returns([]);
    }

    [TestMethod]
    public void CreateNewAccount_Success_CreatesAccountWithCorrectValues()
    {
        // Arrange
        var dialogViewModel = new NewAccountDialogViewModel
        {
            AccountName = "Checking",
            StartingBalance = new Currency(1000m),
        };

        SetupMockDialogSuccess(dialogViewModel);

        // Act
        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );
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
            StartingBalance = new Currency(0m),
        };

        SetupMockDialogSuccess(dialogViewModel);

        // Act
        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );
        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.HasCount(1, _viewModel.Accounts);
        Assert.AreEqual(0m, _viewModel.Accounts[0].Total.Value);
    }

    [TestMethod]
    public void CreateNewAccount_UserCancelsDialog_DoesNotCreateAccount()
    {
        // Arrange
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Secondary);

        _mockContentDialogFactory.Setup(x => x.Create<NewAccountDialog>()).Returns(fake.Object);

        // Act
        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );
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
            StartingBalance = new Currency(100m),
        };

        var secondAccount = new NewAccountDialogViewModel
        {
            AccountName = "Second Account",
            StartingBalance = new Currency(200m),
        };

        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );

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
            StartingBalance = new Currency(100m),
        };

        SetupMockDialogSuccess(dialogViewModel);

        // Act
        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );
        Assert.IsFalse(_viewModel.TransactionsEnabled); // Initially disabled

        _viewModel.CreateNewAccountCommand.Execute(null);

        // Assert
        Assert.IsTrue(_viewModel.TransactionsEnabled);
    }

    private void SetupMockDialogSuccess(NewAccountDialogViewModel dialogViewModel)
    {
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewAccountDialogViewModel;
                    vm?.AccountName = dialogViewModel.AccountName;
                    vm?.StartingBalance = dialogViewModel.StartingBalance;
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<NewAccountDialog>()).Returns(fake.Object);
    }
}
