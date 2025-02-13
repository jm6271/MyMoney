using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Moq;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Core.FS.Models;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class BudgetViewModelTest
    {
        [TestMethod]
        public void Test_NoDefaultBudgetLoaded()
        {
            BudgetViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());
            Assert.IsNull(viewModel.CurrentBudget);
        }

        [TestMethod]
        public void Test_UpdateBudgetTotals()
        {
            BudgetViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());

            // Add a budget for the current month
            NewBudgetDialogViewModel dialogViewModel = new()
            {
                SelectedDateIndex = 0,
                UseLastMonthsBudget = false
            };
            dialogViewModel.SelectedDate = dialogViewModel.AvailableBudgetDates[dialogViewModel.SelectedDateIndex];
            viewModel.AddNewBudget(dialogViewModel);

            // Make sure the budget exists
            Assert.IsNotNull(viewModel.CurrentBudget);

            // Add some items
            BudgetItem income1 = new()
            {
                Amount = new(1000),
                Category = "Income"
            };
            BudgetItem income2 = new()
            {
                Amount = new(500),
                Category = "Income 2"
            };
            
            viewModel.CurrentBudget.BudgetIncomeItems.Add(income1);
            viewModel.CurrentBudget.BudgetIncomeItems.Add(income2);

            BudgetItem Expense1 = new() { Category = "Housing", Amount = new(500) };
            BudgetItem Expense2 = new() { Category = "Food", Amount = new(200) };
            BudgetItem Expense3 = new() { Category = "Misc", Amount = new(800) };

            viewModel.CurrentBudget.BudgetExpenseItems.Add(Expense1);
            viewModel.CurrentBudget.BudgetExpenseItems.Add(Expense2);
            viewModel.CurrentBudget.BudgetExpenseItems.Add(Expense3);


            viewModel.UpdateListViewTotals();

            Assert.AreEqual(1500, viewModel.IncomeTotal.Value);
            Assert.AreEqual(1500, viewModel.ExpenseTotal.Value);
        }

        [TestMethod]
        public void Test_NewBudgetThisMonth()
        {
            BudgetViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());

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
            Assert.AreEqual(DateTime.Today.Date.AddDays(-(DateTime.Today.Date.Day - 1)), viewModel.CurrentBudget.BudgetDate.Date);
        }

        [TestMethod]
        public void Test_NewBudgetNextMonth()
        {
            BudgetViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());

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
