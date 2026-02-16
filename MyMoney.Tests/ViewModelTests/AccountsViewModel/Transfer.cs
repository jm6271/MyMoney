using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Automation;
using LiteDB;
using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Tests;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class TransferTest
    {
        private Mock<IContentDialogService> _mockContentDialogService;
        private Mock<IMessageBoxService> _mockMessageBoxService;
        private Mock<IContentDialogFactory> _mockContentDialogFactory;
        private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

        DatabaseManager _databaseManager;

        [TestInitialize]
        public async Task Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockContentDialogFactory = new Mock<IContentDialogFactory>();

            _databaseManager = new(new MemoryStream());

            _databaseManager.WriteCollection<Account>("Accounts", [
                new Account { AccountName = "Checking", Total = new Currency(1000m) },
                new Account { AccountName = "Savings", Total = new Currency(500m) }
            ]);



            _viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _mockContentDialogService.Object,
                _databaseManager,
                _mockMessageBoxService.Object,
                _mockContentDialogFactory.Object
            );
            await _viewModel.OnNavigatedToAsync();
        }

        [TestMethod]
        public async Task TransferBetweenAccounts_SuccessfulTransfer()
        {
            // Arrange
            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fake.Object.DataContext as TransferDialogViewModel;
                        vm?.TransferFrom = "Checking";
                        vm?.TransferTo = "Savings";
                        vm?.Amount = new Currency(100m);
                    }
                )
                .ReturnsAsync(ContentDialogResult.Primary);

            _mockContentDialogFactory.Setup(x => x.Create<TransferDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.TransferBetweenAccountsCommand.ExecuteAsync(null);

            // Assert
            var account1 = _viewModel.Accounts.First(a => a.AccountName == "Checking");
            var account2 = _viewModel.Accounts.First(a => a.AccountName == "Savings");

            Assert.AreEqual(900m, account1.Total.Value); // 1000 - 100
            Assert.AreEqual(600m, account2.Total.Value); // 500 + 100

            // Verify transactions created in both accounts
            _viewModel.SelectedAccount = account1;
            await _viewModel.LoadTransactions();
            Assert.HasCount(1, _viewModel.SelectedAccountTransactions);

            _viewModel.SelectedAccount = account2;
            await _viewModel.LoadTransactions();
            Assert.HasCount(1, _viewModel.SelectedAccountTransactions);

            // Verify FROM transaction details
            _viewModel.SelectedAccount = account1;
            await _viewModel.SelectedAccountChanged();
            //var fromTransactions = _viewModel.SelectedAccountTransactions.Where(t => t.AccountId == account1.Id).ToList();
            Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
            Assert.AreEqual(-100m, _viewModel.SelectedAccountTransactions[0].Amount.Value);
            Assert.AreEqual("Transfer to Savings", _viewModel.SelectedAccountTransactions[0].Payee);
            Assert.AreEqual("Transfer", _viewModel.SelectedAccountTransactions[0].Memo);

            // Verify TO transaction details
            _viewModel.SelectedAccount = account2;
            await _viewModel.SelectedAccountChanged();
            Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
            Assert.AreEqual(100m, _viewModel.SelectedAccountTransactions[0].Amount.Value);
            Assert.AreEqual("Transfer from Checking", _viewModel.SelectedAccountTransactions[0].Payee);
            Assert.AreEqual("Transfer", _viewModel.SelectedAccountTransactions[0].Memo);
        }

        [TestMethod]
        public async Task TransferBetweenAccounts_CancelledTransfer()
        {
            // Arrange
            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fake.Object.DataContext as TransferDialogViewModel;
                        vm?.TransferFrom = "Checking";
                        vm?.TransferTo = "Savings";
                        vm?.Amount = new Currency(100m);
                    }
                )
                .ReturnsAsync(ContentDialogResult.Secondary);

            _mockContentDialogFactory.Setup(x => x.Create<TransferDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.TransferBetweenAccountsCommand.ExecuteAsync(null);

            // Assert
            var account1 = _viewModel.Accounts.First(a => a.AccountName == "Checking");
            var account2 = _viewModel.Accounts.First(a => a.AccountName == "Savings");

            Assert.AreEqual(1000m, account1.Total.Value); // Should remain unchanged
            Assert.AreEqual(500m, account2.Total.Value); // Should remain unchanged

            _viewModel.SelectedAccount = account1;
            await _viewModel.SelectedAccountChanged();
            Assert.IsEmpty(_viewModel.SelectedAccountTransactions); // No transactions should be created

            _viewModel.SelectedAccount = account2;
            await _viewModel.SelectedAccountChanged();
            Assert.IsEmpty(_viewModel.SelectedAccountTransactions);
        }

        [TestMethod]
        public async Task TransferBetweenAccounts_VerifyTransactionDates()
        {
            // Arrange
            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fake.Object.DataContext as TransferDialogViewModel;
                        vm?.TransferFrom = "Checking";
                        vm?.TransferTo = "Savings";
                        vm?.Amount = new Currency(100m);
                    }
                )
                .ReturnsAsync(ContentDialogResult.Primary);

            _mockContentDialogFactory.Setup(x => x.Create<TransferDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.TransferBetweenAccountsCommand.ExecuteAsync(null);

            // Assert
            var account1 = _viewModel.Accounts.First(a => a.AccountName == "Checking");
            var account2 = _viewModel.Accounts.First(a => a.AccountName == "Savings");

            _viewModel.SelectedAccount = account1;
            await _viewModel.SelectedAccountChanged();
            Assert.AreEqual(DateTime.Today, _viewModel.SelectedAccountTransactions[0].Date);

            _viewModel.SelectedAccount = account2;
            await _viewModel.SelectedAccountChanged();
            Assert.AreEqual(DateTime.Today, _viewModel.SelectedAccountTransactions[0].Date);
        }

        [TestMethod]
        public async Task TransferBetweenAccounts_InsufficientFunds()
        {
            // Arrange
            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fake.Object.DataContext as TransferDialogViewModel;
                        vm?.TransferFrom = "Checking";
                        vm?.TransferTo = "Savings";
                        vm?.Amount = new Currency(1100m);
                    }
                )
                .ReturnsAsync(ContentDialogResult.Primary);

            _mockContentDialogFactory.Setup(x => x.Create<TransferDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.TransferBetweenAccountsCommand.ExecuteAsync(null);

            // Assert
            var account1 = _viewModel.Accounts.First(a => a.AccountName == "Checking");
            var account2 = _viewModel.Accounts.First(a => a.AccountName == "Savings");

            Assert.AreEqual(1000m, account1.Total.Value); // Should remain unchanged
            Assert.AreEqual(500m, account2.Total.Value); // Should remain unchanged
            
            _viewModel.SelectedAccount = account1;
            await _viewModel.SelectedAccountChanged();
            Assert.IsEmpty(_viewModel.SelectedAccountTransactions);
            
            _viewModel.SelectedAccount = account2;
            await _viewModel.SelectedAccountChanged();
            Assert.IsEmpty(_viewModel.SelectedAccountTransactions);

            // Verify message box was shown for insufficient funds
            _mockMessageBoxService.Verify(
                x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once
            );
        }
    }
}
