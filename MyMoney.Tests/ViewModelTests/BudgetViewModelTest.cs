using MyMoney.ViewModels.Pages;
using Wpf.Ui;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class BudgetViewModelTest
    {
        [TestMethod]
        public void Test_CurrentBudgetLoaded()
        {
            BudgetViewModel viewModel = new(new ContentDialogService());
            Assert.IsNotNull(viewModel.CurrentBudget);
        }

        [TestMethod]
        public void Test_UpdateBudgetTotals()
        {
            BudgetViewModel viewModel = new(new ContentDialogService());
            viewModel.UpdateListViewTotals();

            Assert.AreEqual(2020, viewModel.IncomeTotal.Value);
            Assert.AreEqual(2020, viewModel.ExpenseTotal.Value);
        }
    }
}
