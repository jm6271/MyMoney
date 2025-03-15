using MyMoney.ViewModels.Pages;
using MyMoney.Core.FS.Models;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Services.ContentDialogs;
using Moq;
using Wpf.Ui.Controls;
using Wpf.Ui;
using MyMoney.Core.Database;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class RenameAccount
    {
        private Mock<IContentDialogService> _contentDialogService;
        private Mock<IDatabaseReader> _databaseReader;
        private Mock<INewAccountDialogService> _newAccountDialogService;
        private Mock<ITransferDialogService> _transferDialogService;
        private Mock<ITransactionDialogService> _transactionDialogService;
        private Mock<IRenameAccountDialogService> _renameAccountDialogService;
        private Mock<IMessageBoxService> _messageBoxService;

        [TestInitialize]
        public void Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _databaseReader = new Mock<IDatabaseReader>();
            _databaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns(new List<Account>());
            _newAccountDialogService = new Mock<INewAccountDialogService>();
            _transferDialogService = new Mock<ITransferDialogService>();
            _transactionDialogService = new Mock<ITransactionDialogService>();
            _renameAccountDialogService = new Mock<IRenameAccountDialogService>();
            _messageBoxService = new Mock<IMessageBoxService>();
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
                _transactionDialogService.Object,
                _renameAccountDialogService.Object,
                _messageBoxService.Object
            );

            var account = new Account { AccountName = "Old Name" };
            viewModel.Accounts.Add(account);
            viewModel.SelectedAccountIndex = 0;

            var renameViewModel = new RenameAccountViewModel { NewName = "New Name" };
            _renameAccountDialogService.Setup(x => x.GetViewModel()).Returns(renameViewModel);
            _renameAccountDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
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
                _transactionDialogService.Object,
                _renameAccountDialogService.Object,
                _messageBoxService.Object
            );

            var account = new Account { AccountName = "Original Name" };
            viewModel.Accounts.Add(account);
            viewModel.SelectedAccountIndex = 0;

            var renameViewModel = new RenameAccountViewModel { NewName = "New Name" };
            _renameAccountDialogService.Setup(x => x.GetViewModel()).Returns(renameViewModel);
            _renameAccountDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.None);

            // Act
            await viewModel.RenameAccountCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual("Original Name", viewModel.Accounts[0].AccountName);
        }
    }
}
