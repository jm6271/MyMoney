using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class EditTransactionTests
    {
        private Mock<IContentDialogService> _contentDialogService;
        private Mock<IDatabaseManager> _databaseReader;
        private Mock<IMessageBoxService> _messageBoxService;
        private Mock<IContentDialogFactory> _contentDialogFactory;
        private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _databaseReader = new Mock<IDatabaseManager>();
            _messageBoxService = new Mock<IMessageBoxService>();
            _contentDialogFactory = new Mock<IContentDialogFactory>();

            _databaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns([]);

            _viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseReader.Object,
                _messageBoxService.Object,
                _contentDialogFactory.Object
            );
        }

        [TestMethod]
        public async Task EditTransaction_DialogCanceled_DoesNotUpdateTransaction()
        {
            // Arrange
            var account = new Account { AccountName = "Test", Total = new Currency(1000) };
            var transaction = new Transaction(
                DateTime.Today,
                "Test",
                new() { Name = "Category", Group = "Income" },
                new Currency(-100),
                "Memo"
            );
            account.Transactions.Add(transaction);

            _viewModel.Accounts.Add(account);
            _viewModel.SelectedAccount = account;
            _viewModel.SelectedTransactionIndex = 0;

            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Secondary);

            _contentDialogFactory.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.EditTransactionCommand.ExecuteAsync(null);

            // Assert
            Assert.HasCount(1, account.Transactions);
            Assert.AreEqual(1000, account.Total.Value);
        }

        [TestMethod]
        public async Task EditTransaction_UpdatesAmountAndBalance()
        {
            // Arrange
            var account = new Account { AccountName = "Test", Total = new Currency(1000) };
            var oldTransaction = new Transaction(
                DateTime.Today,
                "Test",
                new() { Name = "Category", Group = "Income" },
                new Currency(-100),
                "Memo"
            );
            account.Transactions.Add(oldTransaction);

            _viewModel.Accounts.Add(account);
            _viewModel.SelectedAccount = account;
            _viewModel.SelectedTransactionIndex = 0;

            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fake.Object.DataContext as NewTransactionDialogViewModel;
                        vm?.NewTransactionAmount = new Currency(200);
                        vm?.NewTransactionIsExpense = true;
                        vm?.NewTransactionDate = DateTime.Today;
                        vm?.NewTransactionCategory = new() { Name = "Category", Group = "Income" };
                        vm?.NewTransactionMemo = "Updated memo";
                        vm?.NewTransactionPayee = "Test";
                    }
                )
                .ReturnsAsync(ContentDialogResult.Primary);

            _contentDialogFactory.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.EditTransactionCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(-200, account.Transactions[0].Amount.Value);
            Assert.AreEqual(900, account.Total.Value);
        }
    }
}
