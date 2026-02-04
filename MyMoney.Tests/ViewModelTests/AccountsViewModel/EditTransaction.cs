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
        private Mock<IMessageBoxService> _messageBoxService;
        private Mock<IContentDialogFactory> _contentDialogFactory;
        private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

        private DatabaseManager _databaseManager;

        [TestInitialize]
        public async Task Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _messageBoxService = new Mock<IMessageBoxService>();
            _contentDialogFactory = new Mock<IContentDialogFactory>();

            _databaseManager = new(new MemoryStream());

            _databaseManager.WriteCollection("Accounts", [
                new Account { AccountName = "Test", Total = new Currency(1000m), Id = 1 },
            ]);

            _databaseManager.WriteCollection<Transaction>("Transactions", [
                new Transaction(
                    DateTime.Today,
                    "Test",
                    new() { Name = "Category", Group = "Income" },
                    new Currency(-100m),
                    "Memo"
                )
                {
                    AccountId = 1
                },
            ]);

            _viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseManager,
                _messageBoxService.Object,
                _contentDialogFactory.Object
            );

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
            _databaseManager?.Dispose();
        }

        [TestMethod]
        public async Task EditTransaction_DialogCanceled_DoesNotUpdateTransaction()
        {
            // Arrange
            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Secondary);

            _contentDialogFactory.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.EditTransactionCommand.ExecuteAsync(null);

            // Assert
            Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
            Assert.AreEqual(1000, _viewModel.Accounts[0].Total.Value);
        }

        [TestMethod]
        public async Task EditTransaction_UpdatesAmountAndBalance()
        {
            // Arrange
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
            Assert.AreEqual(-200, _viewModel.SelectedAccountTransactions[0].Amount.Value);
            Assert.AreEqual(900, _viewModel.Accounts[0].Total.Value);
        }
    }
}
