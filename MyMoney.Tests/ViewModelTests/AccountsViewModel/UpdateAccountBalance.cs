using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel
{
    [TestClass]
    public class UpdateAccountBalanceTests
    {
        private Mock<IContentDialogService> _contentDialogService;
        private Mock<IDatabaseManager> _databaseReader;
        private Mock<IMessageBoxService> _messageBoxService;
        private Mock<IContentDialogFactory> _contentDialogFactory;

        private ViewModels.Pages.AccountsViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _databaseReader = new Mock<IDatabaseManager>();
            _databaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns(new List<Account>());
            _messageBoxService = new Mock<IMessageBoxService>();
            _contentDialogFactory = new Mock<IContentDialogFactory>();

            _viewModel = new ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseReader.Object,
                _messageBoxService.Object,
                _contentDialogFactory.Object
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

            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    var vm = fake.Object.DataContext as UpdateAccountBalanceDialogViewModel;
                    vm?.Balance = new Currency(150);
                })
                .ReturnsAsync(ContentDialogResult.Primary);

            _contentDialogFactory.Setup(x => x.Create<UpdateAccountBalanceDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.UpdateAccountBalanceCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(150m, _viewModel.Accounts[0].Total.Value);
        }
    }
}
