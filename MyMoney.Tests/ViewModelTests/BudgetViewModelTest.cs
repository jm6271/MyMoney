using MyMoney.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class BudgetViewModelTest
    {
        [TestMethod]
        public void Test_CurrentBudgetLoaded()
        {
            BudgetViewModel viewModel = new();
            Assert.IsNotNull(viewModel.CurrentBudget);
        }

        [TestMethod]
        public void Test_UpdateBudgetTotals()
        {
            BudgetViewModel viewModel = new();
            viewModel.UpdateListViewTotals();

            Assert.AreEqual(2020, viewModel.IncomeTotal.Value);
            Assert.AreEqual(2020, viewModel.ExpenseTotal.Value);
        }
    }
}
