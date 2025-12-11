using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class RenameAccount
    {
        private Mock<IContentDialogService> _contentDialogService;
        private Mock<IDatabaseManager> _databaseReader;
        private Mock<INewAccountDialogService> _newAccountDialogService;
        private Mock<ITransferDialogService> _transferDialogService;
        private Mock<IRenameAccountDialogService> _renameAccountDialogService;
        private Mock<IMessageBoxService> _messageBoxService;
        private Mock<IUpdateAccountBalanceDialogService> _updateAccountBalanceDialogService;
        private Mock<IContentDialogFactory> _contentDialogFactory;

        [TestInitialize]
        public void Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _databaseReader = new Mock<IDatabaseManager>();
            _databaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns(new List<Account>());
            _newAccountDialogService = new Mock<INewAccountDialogService>();
            _transferDialogService = new Mock<ITransferDialogService>();
            _renameAccountDialogService = new Mock<IRenameAccountDialogService>();
            _messageBoxService = new Mock<IMessageBoxService>();
            _updateAccountBalanceDialogService = new Mock<IUpdateAccountBalanceDialogService>();
            _contentDialogFactory = new Mock<IContentDialogFactory>();
        }

        [TestMethod]
        public async Task RenameAccount_SuccessfulRename_UpdatesAccountName()
        {
            // Arrange
            var viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseReader.Object,
                _newAccountDialogService.Object,
                _transferDialogService.Object,
                _renameAccountDialogService.Object,
                _messageBoxService.Object,
                _updateAccountBalanceDialogService.Object,
                _contentDialogFactory.Object
            );

            var account = new Account { AccountName = "Old Name" };
            viewModel.Accounts.Add(account);
            viewModel.SelectedAccountIndex = 0;

            var renameViewModel = new RenameAccountViewModel { NewName = "New Name" };
            _renameAccountDialogService.Setup(x => x.GetViewModel()).Returns(renameViewModel);
            _renameAccountDialogService
                .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.Primary);

            // Act
            await viewModel.RenameAccountCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual("New Name", viewModel.Accounts[0].AccountName);
        }

        [TestMethod]
        public async Task RenameAccount_CancelRename_MaintainsOriginalName()
        {
            // Arrange
            var viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseReader.Object,
                _newAccountDialogService.Object,
                _transferDialogService.Object,
                _renameAccountDialogService.Object,
                _messageBoxService.Object,
                _updateAccountBalanceDialogService.Object,
                _contentDialogFactory.Object
            );

            var account = new Account { AccountName = "Original Name" };
            viewModel.Accounts.Add(account);
            viewModel.SelectedAccountIndex = 0;

            var renameViewModel = new RenameAccountViewModel { NewName = "New Name" };
            _renameAccountDialogService.Setup(x => x.GetViewModel()).Returns(renameViewModel);
            _renameAccountDialogService
                .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.None);

            // Act
            await viewModel.RenameAccountCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual("Original Name", viewModel.Accounts[0].AccountName);
        }
    }
}
