using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using Moq;
using Wpf.Ui.Controls;
using MyMoney.Core.Database;
using Wpf.Ui;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class EditTransactionTests
    {
        private Mock<IContentDialogService> _contentDialogService;
        private Mock<IDatabaseReader> _databaseReader;
        private Mock<INewAccountDialogService> _accountDialogService;
        private Mock<ITransferDialogService> _transferDialogService;
        private Mock<ITransactionDialogService> _transactionDialogService;
        private Mock<IRenameAccountDialogService> _renameAccountDialogService;
        private Mock<IMessageBoxService> _messageBoxService;
        private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _databaseReader = new Mock<IDatabaseReader>();
            _accountDialogService = new Mock<INewAccountDialogService>();
            _transferDialogService = new Mock<ITransferDialogService>();
            _transactionDialogService = new Mock<ITransactionDialogService>();
            _renameAccountDialogService = new Mock<IRenameAccountDialogService>();
            _messageBoxService = new Mock<IMessageBoxService>();

            _databaseReader.Setup(x => x.GetCollection<Account>("Accounts"))
                .Returns([]);
            
            _viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseReader.Object,
                _accountDialogService.Object,
                _transferDialogService.Object,
                _transactionDialogService.Object,
                _renameAccountDialogService.Object,
                _messageBoxService.Object
            );
        }

        [TestMethod]
        public async Task EditTransaction_NoSelectedAccount_ReturnsEarly()
        {
            // Arrange
            _viewModel.SelectedAccount = null;
            _viewModel.SelectedTransactionIndex = 0;

            // Act
            await _viewModel.EditTransactionCommand.ExecuteAsync(null);

            // Assert
            _transactionDialogService.Verify(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()), Times.Never);
        }

        [TestMethod]
        public async Task EditTransaction_NoSelectedTransaction_ReturnsEarly()
        {
            // Arrange
            _viewModel.SelectedAccount = new Account { AccountName = "Test" };
            _viewModel.SelectedTransactionIndex = -1;

            // Act
            await _viewModel.EditTransactionCommand.ExecuteAsync(null);

            // Assert
            _transactionDialogService.Verify(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()), Times.Never);
        }

        [TestMethod]
        public async Task EditTransaction_DialogCanceled_DoesNotUpdateTransaction()
        {
            // Arrange
            var account = new Account { AccountName = "Test", Total = new Currency(1000) };
            var transaction = new Transaction(DateTime.Today, "Test", new() { Name = "Category", Group = "Income" }, new Currency(-100), "Memo");
            account.Transactions.Add(transaction);
            
            _viewModel.Accounts.Add(account);
            _viewModel.SelectedAccount = account;
            _viewModel.SelectedTransactionIndex = 0;

            _transactionDialogService
                .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.Secondary);

            // Act
            await _viewModel.EditTransactionCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(-100, account.Transactions[0].Amount.Value);
            Assert.AreEqual(1000, account.Total.Value);
        }

        [TestMethod]
        public async Task EditTransaction_UpdatesAmountAndBalance()
        {
            // Arrange
            var account = new Account { AccountName = "Test", Total = new Currency(1000) };
            var oldTransaction = new Transaction(DateTime.Today, "Test", new() { Name = "Category", Group = "Income" }, new Currency(-100), "Memo");
            account.Transactions.Add(oldTransaction);
            
            _viewModel.Accounts.Add(account);
            _viewModel.SelectedAccount = account;
            _viewModel.SelectedTransactionIndex = 0;
            
            _transactionDialogService
                .Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.Primary);
            _transactionDialogService
                .Setup(x => x.GetViewModel())
                .Returns(new NewTransactionDialogViewModel()
                {
                    NewTransactionAmount = new Currency(200),
                    NewTransactionIsExpense = true,
                    NewTransactionDate = DateTime.Today,
                    NewTransactionCategory = new() { Name = "Category", Group = "Income" },
                    NewTransactionMemo = "Updated memo"
                });
            _transactionDialogService
                .Setup(x => x.GetSelectedPayee())
                .Returns("Test");

            // Act
            await _viewModel.EditTransactionCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(-200, account.Transactions[0].Amount.Value);
            Assert.AreEqual(900, account.Total.Value);
        }
    }
}
