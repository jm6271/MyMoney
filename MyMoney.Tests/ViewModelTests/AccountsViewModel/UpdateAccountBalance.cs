using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class UpdateAccountBalanceTests
    {
        private Mock<IContentDialogService> _contentDialogService;
        private Mock<IDatabaseManager> _databaseReader;
        private Mock<INewAccountDialogService> _newAccountDialogService;
        private Mock<ITransferDialogService> _transferDialogService;
        private Mock<ITransactionDialogService> _transactionDialogService;
        private Mock<IRenameAccountDialogService> _renameAccountDialogService;
        private Mock<IMessageBoxService> _messageBoxService;
        private Mock<IUpdateAccountBalanceDialogService> _updateAccountBalanceDialogService;

        private ViewModels.Pages.AccountsViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _databaseReader = new Mock<IDatabaseManager>();
            _databaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns(new List<Account>());
            _newAccountDialogService = new Mock<INewAccountDialogService>();
            _transferDialogService = new Mock<ITransferDialogService>();
            _transactionDialogService = new Mock<ITransactionDialogService>();
            _renameAccountDialogService = new Mock<IRenameAccountDialogService>();
            _messageBoxService = new Mock<IMessageBoxService>();
            _updateAccountBalanceDialogService = new Mock<IUpdateAccountBalanceDialogService>();

            _viewModel = new ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseReader.Object,
                _newAccountDialogService.Object,
                _transferDialogService.Object,
                _transactionDialogService.Object,
                _renameAccountDialogService.Object,
                _messageBoxService.Object,
                _updateAccountBalanceDialogService.Object
            );
        }

        [TestMethod]
        public async Task UpdateAccountBalance_BalanceChanged_UpdatesBalanceCorrectly()
        {
            // Arrange
            var account = new Account { Total = new Currency(100) };
            _viewModel.Accounts.Add(account);
            _viewModel.SelectedAccountIndex = 0;
            _viewModel.SelectedAccount = _viewModel.Accounts[0];

            _updateAccountBalanceDialogService
                .Setup(x => x.GetViewModel())
                .Returns(new UpdateAccountBalanceDialogViewModel { Balance = new Currency(150) });
            _updateAccountBalanceDialogService
                .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(Wpf.Ui.Controls.ContentDialogResult.Primary);

            // Act
            await _viewModel.UpdateAccountBalanceCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(150m, _viewModel.Accounts[0].Total.Value);
        }
    }
}
