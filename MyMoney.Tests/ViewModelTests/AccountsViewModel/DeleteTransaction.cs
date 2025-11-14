using MyMoney.Core.Models;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Moq;
using MyMoney.Services.ContentDialogs;
using MyMoney.Core.Database;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class DeleteTransactionTest
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseService;
    private Mock<INewAccountDialogService> _mockNewAccountDialogService;
    private Mock<ITransactionDialogService> _mockTransactionDialogService;
    private Mock<IRenameAccountDialogService> _mockRenameAccountDialogService;
    private Mock<ITransferDialogService> _mockTransferDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<IUpdateAccountBalanceDialogService> _mockUpdateAccountBalanceDialogService;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

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

        // Setup empty accounts collection by default
        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts"))
            .Returns([]);
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
        _mockMessageBoxService.Verify(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never());
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
        _mockMessageBoxService.Verify(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public void DeleteTransaction_UserClicksNo_DoesNotDeleteTransaction()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m) };
        var transaction = new Transaction(DateTime.Today, "Test", 
            new() { Name = "Category", Group = "Income" }, new Currency(100m), "Memo");
        account.Transactions.Add(transaction);
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        _viewModel.SelectedTransactionIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.HasCount(1, account.Transactions);
        Assert.AreEqual(1000m, account.Total.Value);
    }

    [TestMethod]
    public void DeleteTransaction_UserConfirmsDelete_RemovesTransactionAndUpdatesBalance()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m) };
        var transaction = new Transaction(DateTime.Today, "Test", 
            new() { Name = "Category", Group = "Income" }, new Currency(100m), "Memo");
        account.Transactions.Add(transaction);
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        _viewModel.SelectedTransactionIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.HasCount(0, account.Transactions);
        Assert.AreEqual(900m, account.Total.Value); // 1000 - 100
    }

    [TestMethod]
    public void DeleteTransaction_WithNegativeAmount_CorrectlyUpdatesBalance()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m) };
        var transaction = new Transaction(DateTime.Today, "Test", 
            new() { Name = "Category", Group = "Income" }, new Currency(-100m), "Memo");
        account.Transactions.Add(transaction);
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccount = account;
        _viewModel.SelectedTransactionIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.IsEmpty(account.Transactions);
        Assert.AreEqual(1100m, account.Total.Value); // 1000 - (-100)
    }

    private MyMoney.ViewModels.Pages.AccountsViewModel CreateViewModel()
    {
        return new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockNewAccountDialogService.Object,
            _mockTransferDialogService.Object,
            _mockTransactionDialogService.Object,
            _mockRenameAccountDialogService.Object,
            _mockMessageBoxService.Object,
            _mockUpdateAccountBalanceDialogService.Object
        );
    }
}
