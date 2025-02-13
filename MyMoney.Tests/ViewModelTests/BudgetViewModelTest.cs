using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Moq;
using MyMoney.ViewModels.ContentDialogs;

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

        [TestMethod]
        public void Test_NewBudgetThisMonth()
        {
            BudgetViewModel viewModel = new(new ContentDialogService());

            // Create fake dialog results for the new budget dialog
            NewBudgetDialogViewModel dialogViewModel = new()
            {
                SelectedDateIndex = 0
            };

            dialogViewModel.SelectedDate = dialogViewModel.AvailableBudgetDates[dialogViewModel.SelectedDateIndex];
            dialogViewModel.UseLastMonthsBudget = false;

            // Create a new budget
            viewModel.AddNewBudget(dialogViewModel);

            // Make sure the budget got added
            Assert.IsNotNull(viewModel.CurrentBudget);
            Assert.AreEqual(DateTime.Today.Date, viewModel.CurrentBudget.BudgetDate.Date);
        }

        [TestMethod]
        public void Test_NewBudgetNextMonth()
        {
            BudgetViewModel viewModel = new(new ContentDialogService());

            // Create fake dialog results for the new budget dialog
            NewBudgetDialogViewModel dialogViewModel = new()
            {
                SelectedDateIndex = 1,
                UseLastMonthsBudget = false
            };

            dialogViewModel.SelectedDate = dialogViewModel.AvailableBudgetDates[dialogViewModel.SelectedDateIndex];

            // Create a new budget
            viewModel.AddNewBudget(dialogViewModel);

            // Make sure the budget got added
            Assert.IsNotNull(viewModel.CurrentBudget);
            Assert.AreEqual(DateTime.Today.Date.AddMonths(1).AddDays(-(DateTime.Today.Date.Day - 1)), 
                viewModel.CurrentBudget.BudgetDate.Date);
        }
    }
}
