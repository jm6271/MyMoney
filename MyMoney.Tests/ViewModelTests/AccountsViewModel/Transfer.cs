using MyMoney.ViewModels.Pages;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using Moq;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MyMoney.Core.Database;
using System.Collections.ObjectModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class TransferTest
    {
        private Mock<IContentDialogService> _mockContentDialogService;
        private Mock<IDatabaseManager> _mockDatabaseReader;
        private Mock<INewAccountDialogService> _mockNewAccountDialogService;
        private Mock<ITransferDialogService> _mockTransferDialogService;
        private Mock<ITransactionDialogService> _mockTransactionDialogService;
        private Mock<IRenameAccountDialogService> _mockRenameAccountDialogService;
        private Mock<IMessageBoxService> _mockMessageBoxService;
        private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockDatabaseReader = new Mock<IDatabaseManager>();
            _mockNewAccountDialogService = new Mock<INewAccountDialogService>();
            _mockTransferDialogService = new Mock<ITransferDialogService>();
            _mockTransactionDialogService = new Mock<ITransactionDialogService>();
            _mockRenameAccountDialogService = new Mock<IRenameAccountDialogService>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();

            // Set up empty accounts collection in database reader
            _mockDatabaseReader.Setup(x => x.GetCollection<Account>("Accounts"))
                .Returns(new List<Account>());

            _viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockNewAccountDialogService.Object,
                _mockTransferDialogService.Object,
                _mockTransactionDialogService.Object,
                _mockRenameAccountDialogService.Object,
                _mockMessageBoxService.Object
            );
        }

        [TestMethod]
        public void TransferBetweenAccounts_SuccessfulTransfer()
        {
            // Arrange
            var account1 = new Account { AccountName = "Checking", Total = new Currency(1000m) };
            var account2 = new Account { AccountName = "Savings", Total = new Currency(500m) };
            _viewModel.Accounts.Add(account1);
            _viewModel.Accounts.Add(account2);

            var transferViewModel = new TransferDialogViewModel(new ObservableCollection<string> { "Checking", "Savings" })
            {
                TransferFrom = "Checking",
                TransferTo = "Savings",
                Amount = new Currency(100m)
            };

            _mockTransferDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.Primary);
            _mockTransferDialogService.Setup(x => x.GetViewModel())
                .Returns(transferViewModel);

            // Act
            _viewModel.TransferBetweenAccountsCommand.Execute(null);

            // Assert
            Assert.AreEqual(900m, account1.Total.Value); // 1000 - 100
            Assert.AreEqual(600m, account2.Total.Value); // 500 + 100

            // Verify transactions created in both accounts
            Assert.AreEqual(1, account1.Transactions.Count);
            Assert.AreEqual(1, account2.Transactions.Count);

            // Verify FROM transaction details
            var fromTransaction = account1.Transactions[0];
            Assert.AreEqual(-100m, fromTransaction.Amount.Value);
            Assert.AreEqual("Transfer to Savings", fromTransaction.Payee);
            Assert.AreEqual("Transfer", fromTransaction.Memo);

            // Verify TO transaction details
            var toTransaction = account2.Transactions[0];
            Assert.AreEqual(100m, toTransaction.Amount.Value);
            Assert.AreEqual("Transfer from Checking", toTransaction.Payee);
            Assert.AreEqual("Transfer", toTransaction.Memo);
        }

        [TestMethod]
        public void TransferBetweenAccounts_CancelledTransfer()
        {
            // Arrange
            var account1 = new Account { AccountName = "Checking", Total = new Currency(1000m) };
            var account2 = new Account { AccountName = "Savings", Total = new Currency(500m) };
            _viewModel.Accounts.Add(account1);
            _viewModel.Accounts.Add(account2);

            _mockTransferDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.None); // User cancels dialog

            // Act
            _viewModel.TransferBetweenAccountsCommand.Execute(null);

            // Assert
            Assert.AreEqual(1000m, account1.Total.Value); // Should remain unchanged
            Assert.AreEqual(500m, account2.Total.Value);  // Should remain unchanged
            Assert.AreEqual(0, account1.Transactions.Count); // No transactions should be created
            Assert.AreEqual(0, account2.Transactions.Count);
        }

        [TestMethod]
        public void TransferBetweenAccounts_VerifyTransactionDates()
        {
            // Arrange
            var account1 = new Account { AccountName = "Checking", Total = new Currency(1000m) };
            var account2 = new Account { AccountName = "Savings", Total = new Currency(500m) };
            _viewModel.Accounts.Add(account1);
            _viewModel.Accounts.Add(account2);

            var transferViewModel = new TransferDialogViewModel(new ObservableCollection<string> { "Checking", "Savings" })
            {
                TransferFrom = "Checking",
                TransferTo = "Savings",
                Amount = new Currency(100m)
            };

            _mockTransferDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.Primary);
            _mockTransferDialogService.Setup(x => x.GetViewModel())
                .Returns(transferViewModel);

            // Act
            _viewModel.TransferBetweenAccountsCommand.Execute(null);

            // Assert
            Assert.AreEqual(DateTime.Today, account1.Transactions[0].Date);
            Assert.AreEqual(DateTime.Today, account2.Transactions[0].Date);
        }

        [TestMethod]
        public void TransferBetweenAccounts_InsufficientFunds()
        {
            // Arrange
            var account1 = new Account { AccountName = "Checking", Total = new Currency(50m) };
            var account2 = new Account { AccountName = "Savings", Total = new Currency(500m) };
            _viewModel.Accounts.Add(account1);
            _viewModel.Accounts.Add(account2);

            var transferViewModel = new TransferDialogViewModel(new ObservableCollection<string> { "Checking", "Savings" })
            {
                TransferFrom = "Checking",
                TransferTo = "Savings",
                Amount = new Currency(100m) // More than available in Checking
            };

            _mockTransferDialogService.Setup(x => x.ShowDialogAsync(It.IsAny<IContentDialogService>()))
                .ReturnsAsync(ContentDialogResult.Primary);
            _mockTransferDialogService.Setup(x => x.GetViewModel())
                .Returns(transferViewModel);

            // Act
            _viewModel.TransferBetweenAccountsCommand.Execute(null);

            // Assert
            Assert.AreEqual(50m, account1.Total.Value); // Should remain unchanged
            Assert.AreEqual(500m, account2.Total.Value); // Should remain unchanged
            Assert.AreEqual(0, account1.Transactions.Count); // No transactions should be created
            Assert.AreEqual(0, account2.Transactions.Count);

            // Verify message box was shown for insufficient funds
            _mockMessageBoxService.Verify(x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
        }
    }
}
