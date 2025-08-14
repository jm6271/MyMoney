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
public class DeleteAccountTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseService;
    private Mock<INewAccountDialogService> _mockNewAccountDialogService;
    private Mock<ITransactionDialogService> _mockTransactionDialogService;
    private Mock<IRenameAccountDialogService> _mockRenameAccountDialogService;
    private Mock<ITransferDialogService> _mockTransferDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
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

        // Setup empty accounts collection by default
        _mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts"))
            .Returns([]);
    }

    [TestMethod]
    public void DeleteAccount_NoSelectedAccount_DoesNothing()
    {
        // Arrange
        _viewModel = CreateViewModel();
        _viewModel.SelectedAccountIndex = -1;

        // Act
        _viewModel.DeleteAccountCommand.Execute(null);

        // Assert
        _mockMessageBoxService.Verify(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public void DeleteAccount_UserClicksNo_DoesNotDeleteAccount()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account = new Account { AccountName = "Test", Total = new Currency(1000m), Id = 1 };
        _viewModel.Accounts.Add(account);
        _viewModel.SelectedAccountIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Secondary);

        // Act
        _viewModel.DeleteAccountCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, _viewModel.Accounts.Count);
    }

    [TestMethod]
    public void DeleteAccount_UserConfirmsDelete_RemovesAccountAndUpdatesIds()
    {
        // Arrange
        _viewModel = CreateViewModel();
        var account1 = new Account { AccountName = "Test1", Total = new Currency(1000m), Id = 1 };
        var account2 = new Account { AccountName = "Test2", Total = new Currency(2000m), Id = 2 };
        _viewModel.Accounts.Add(account1);
        _viewModel.Accounts.Add(account2);
        _viewModel.SelectedAccountIndex = 0;

        _mockMessageBoxService
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        // Act
        _viewModel.DeleteAccountCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, _viewModel.Accounts.Count);
        Assert.AreEqual(1, _viewModel.Accounts[0].Id);
        Assert.AreEqual("Test2", _viewModel.Accounts[0].AccountName);
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
            _mockMessageBoxService.Object
        );
    }
}