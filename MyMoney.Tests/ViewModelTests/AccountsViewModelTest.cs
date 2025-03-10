using MyMoney.Core.FS.Models;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Moq;
using MyMoney.Views.ContentDialogs;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class AccountsViewModelTest
    {
        [TestMethod]
        public void Test_NewAccount()
        {
            // Create a mock database object
            var mockDatabaseService = new Mock<Core.Database.IDatabaseReader>();
            mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts")).Returns([]);

            // Create a mock content dialog service
            var mockContentDialogService = new Mock<IContentDialogService>();
            mockContentDialogService.Setup(service => service.GetDialogHost()).Returns(new System.Windows.Controls.ContentPresenter());
            mockContentDialogService.Setup(service => service.ShowAsync(It.IsAny<NewAccountDialog>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Wpf.Ui.Controls.ContentDialogResult.Primary);

            AccountsViewModel viewModel = new(mockContentDialogService.Object, mockDatabaseService.Object);
            viewModel.CreateNewAccountCommand.Execute(null);

            Assert.AreEqual(1, viewModel.Accounts.Count);
        }


        [TestMethod]
        public void Test_Transfer()
        {
            AccountsViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());

            // Create some accounts
            Account account1 = new()
            {
                AccountName = "Account1",
                Total = new Currency(2000)
            };
            viewModel.Accounts.Add(account1);

            Account account2 = new()
            {
                AccountName = "Account2",
                Total = new Currency(1000)
            };
            viewModel.Accounts.Add(account2);

            // Transfer
            TransferDialogViewModel dialogViewModel = new(["Account1", "Account2"])
            {
                Amount = new(100),
                TransferFrom = "Account1",
                TransferTo = "Account2"
            };
            
            viewModel.Transfer(dialogViewModel);

            Assert.AreEqual(1900, viewModel.Accounts[0].Total.Value);
            Assert.AreEqual(1100, viewModel.Accounts[1].Total.Value);
        }
    }
}
