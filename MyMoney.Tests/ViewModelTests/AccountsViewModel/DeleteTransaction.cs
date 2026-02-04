using System.Security.Principal;
using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Tests;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class DeleteTransactionTest
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

    private DatabaseManager _databaseManager;

    [TestInitialize]
    public async Task Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        _databaseManager = new(new MemoryStream());
        _databaseManager.WriteCollection("Accounts", [
            new Account { AccountName = "Test", Total = new Currency(1000m), Id = 1 },
        ]);

        _databaseManager.WriteCollection("Transactions", [
            new Transaction(
                DateTime.Today,
                "Test",
                new() { Name = "Category", Group = "Income" },
                new Currency(100m),
                "Memo"
            )
            {
                AccountId = 1
            },
            new Transaction(
                DateTime.Today,
                "Test 2",
                new() { Name = "Category 2", Group = "Expenses" },
                new Currency(-100m),
                "Memo 2"
            )
            {
                AccountId = 1
            },
        ]);

        _viewModel = new(
            _mockContentDialogService.Object,
            _databaseManager,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object);

        _viewModel.SelectedAccount = _viewModel.Accounts[0];
        _viewModel.SelectedAccountIndex = 0;

        await _viewModel.OnNavigatedToAsync();
        await _viewModel.LoadTransactions();

        _viewModel.SelectedTransactionIndex = 0;
        _viewModel.SelectedTransaction = _viewModel.SelectedAccountTransactions[0];
    }

    [TestCleanup]
    public void Cleanup()
    {
        // This forces the in-memory DB to be destroyed
        if (_databaseManager is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [TestMethod]
    public void DeleteTransaction_NoSelectedAccount_DoesNothing()
    {
        // Arrange
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
        _viewModel.SelectedTransactionIndex = -1;
        _viewModel.SelectedTransaction = null;

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
        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.HasCount(2, _viewModel.SelectedAccountTransactions);
        Assert.AreEqual(1000m, _viewModel.Accounts[0].Total.Value);
    }

    [TestMethod]
    public void DeleteTransaction_UserConfirmsDelete_RemovesTransactionAndUpdatesBalance()
    {
        // Arrange
        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions); // 2 - 1
        Assert.AreEqual(900m, _viewModel.Accounts[0].Total.Value); // 1000 - 100
    }

    [TestMethod]
    public void DeleteTransaction_WithNegativeAmount_CorrectlyUpdatesBalance()
    {
        // Arrange
        _viewModel.SelectedTransactionIndex = 1;
        _viewModel.SelectedTransaction = _viewModel.SelectedAccountTransactions[1];

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        _viewModel.DeleteTransactionCommand.Execute(null);

        // Assert
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
        Assert.AreEqual(1100m, _viewModel.Accounts[0].Total.Value); // 1000 - (-100)
    }
}
