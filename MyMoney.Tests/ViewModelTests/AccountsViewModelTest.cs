using MyMoney.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class AccountsViewModelTest
    {
        [TestMethod]
        public void Test_AccountsAreLoaded()
        {
            AccountsViewModel viewModel = new(new ContentDialogService());
            Assert.AreEqual(1, viewModel.Accounts.Count);
        }

        [TestMethod]
        public void Test_CategoryNamesAreLoaded()
        {
            AccountsViewModel viewModel = new(new ContentDialogService());
            Assert.AreEqual(7, viewModel.CategoryNames.Count);
        }
    }
}
