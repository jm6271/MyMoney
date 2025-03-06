using MyMoney.Core.FS.Models;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class AccountsViewModelTest
    {
        [TestMethod]
        public void Test_NoAccountsLoadedByDefault()
        {
            AccountsViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());

            Assert.AreEqual(0, viewModel.Accounts.Count);
            Assert.AreEqual(0, viewModel.CategoryNames.Count);
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
