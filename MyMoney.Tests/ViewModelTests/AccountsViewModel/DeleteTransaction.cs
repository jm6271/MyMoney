using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MyMoney.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class DeleteTransactionTest
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseService = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();

        // Setup empty accounts collection by default
        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts")).Returns([]);
    }

    [TestMethod]
    public void DeleteTransaction_NoSelectedAccount_DoesNothing()
    {
        // Arrange
        _viewModel = CreateViewModel();
        _viewModel.SelectedAccount = null;

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        _mockMessageBoxService.Verify(
            x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never()
        );
    }

    [TestMethod]
    public void DeleteTransaction_NoSelectedTransaction_DoesNothing()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m) };
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        _viewModel.SelectedTransactionIndex = -1;

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        _mockMessageBoxService.Verify(
            x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never()
        );
    }

    [TestMethod]
    public void DeleteTransaction_UserClicksNo_DoesNotDeleteTransaction()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m), Id = 1 };
        var transaction = new Transaction(
            DateTime.Today,
            "Test",
            new() { Name = "Category", Group = "Income" },
            new Currency(100m),
            "Memo"
        )
        {
            AccountId = account.Id
        };
        
        // Setup accounts and transactions
        var transactions = new List<Transaction> { transaction };
        Utils.SetupTransactionsMock(_mockDatabaseService, transactions);
        
        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts"))
            .Returns(new List<Account> { account });

        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        
        // Load transactions for the account
        _viewModel.LoadTransactions().Wait();
        _viewModel.SelectedTransactionIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
        Assert.AreEqual(1000m, account.Total.Value);
    }

    [TestMethod]
    public void DeleteTransaction_UserConfirmsDelete_RemovesTransactionAndUpdatesBalance()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m), Id = 1 };
        var transaction = new Transaction(
            DateTime.Today,
            "Test",
            new() { Name = "Category", Group = "Income" },
            new Currency(100m),
            "Memo"
        )
        {
            AccountId = account.Id
        };
        
        // Setup accounts and transactions
        var transactions = new List<Transaction> { transaction };
        Utils.SetupTransactionsMock(_mockDatabaseService, transactions);
        
        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts"))
            .Returns(new List<Account> { account });

        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        
        // Load transactions for the account
        _viewModel.LoadTransactions().Wait();
        _viewModel.SelectedTransactionIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.HasCount(0, _viewModel.SelectedAccountTransactions);
        Assert.AreEqual(900m, account.Total.Value); // 1000 - 100
    }

    [TestMethod]
    public void DeleteTransaction_WithNegativeAmount_CorrectlyUpdatesBalance()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m), Id = 1 };
        var transaction = new Transaction(
            DateTime.Today,
            "Test",
            new() { Name = "Category", Group = "Income" },
            new Currency(-100m),
            "Memo"
        )
        {
            AccountId = account.Id
        };
        
        // Setup accounts and transactions
        var transactions = new List<Transaction> { transaction };
        Utils.SetupTransactionsMock(_mockDatabaseService, transactions);
        
        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts"))
            .Returns(new List<Account> { account });

        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        
        // Load transactions for the account
        _viewModel.LoadTransactions().Wait();
        _viewModel.SelectedTransactionIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.IsEmpty(_viewModel.SelectedAccountTransactions);
        Assert.AreEqual(1100m, account.Total.Value); // 1000 - (-100)
    }

    private MyMoney.ViewModels.Pages.AccountsViewModel CreateViewModel()
    {
        return new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockMessageBoxService.Object,
            Mock.Of<IContentDialogFactory>()
        );
    }
}
