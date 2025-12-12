using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using MyMoney.Views.ContentDialogs;
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
        private Mock<IMessageBoxService> _messageBoxService;
        private Mock<IContentDialogFactory> _contentDialogFactory;

        [TestInitialize]
        public void Setup()
        {
            _contentDialogService = new Mock<IContentDialogService>();
            _databaseReader = new Mock<IDatabaseManager>();
            _databaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns(new List<Account>());
            _messageBoxService = new Mock<IMessageBoxService>();
            _contentDialogFactory = new Mock<IContentDialogFactory>();
        }

        [TestMethod]
        public async Task RenameAccount_SuccessfulRename_UpdatesAccountName()
        {
            // Arrange
            var viewModel = new MyMoney.ViewModels.Pages.AccountsViewModel(
                _contentDialogService.Object,
                _databaseReader.Object,
                _messageBoxService.Object,
                _contentDialogFactory.Object
            );

            var account = new Account { AccountName = "Old Name" };
            viewModel.Accounts.Add(account);
            viewModel.SelectedAccountIndex = 0;

            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    var vm = fake.Object.DataContext as RenameAccountViewModel;
                    vm?.NewName = "New Name";
                })
                .ReturnsAsync(ContentDialogResult.Primary);

            _contentDialogFactory.Setup(x => x.Create<RenameAccountDialog>()).Returns(fake.Object);

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
                _messageBoxService.Object,
                _contentDialogFactory.Object
            );

            var account = new Account { AccountName = "Original Name" };
            viewModel.Accounts.Add(account);
            viewModel.SelectedAccountIndex = 0;

            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    var vm = fake.Object.DataContext as RenameAccountViewModel;
                    vm?.NewName = "New Name";
                })
                .ReturnsAsync(ContentDialogResult.Secondary);

            _contentDialogFactory.Setup(x => x.Create<RenameAccountDialog>()).Returns(fake.Object);

            // Act
            await viewModel.RenameAccountCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual("Original Name", viewModel.Accounts[0].AccountName);
        }
    }
}
