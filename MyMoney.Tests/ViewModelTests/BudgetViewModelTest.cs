using MyMoney.ViewModels.Pages;
using Wpf.Ui;
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

        [TestMethod]
        public void Test_CreateAndEditExpenseItem()
        {
            BudgetViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());

            NewBudgetDialogViewModel dialogViewModel = new()
            {
                SelectedDateIndex = 0,
                UseLastMonthsBudget = false
            };
            dialogViewModel.SelectedDate = dialogViewModel.AvailableBudgetDates[dialogViewModel.SelectedDateIndex];

            // Create a new budget
            viewModel.AddNewBudget(dialogViewModel);

            Assert.IsNotNull(viewModel.CurrentBudget);

            // Add a new expense item
            BudgetCategoryDialogViewModel expenseItem = new()
            {
                BudgetCategory = "Expense1",
                BudgetAmount = new(500)
            };
            viewModel.CreateNewExpenseItem(expenseItem);

            Assert.AreEqual(1, viewModel.CurrentBudget.BudgetExpenseItems.Count);
            Assert.AreEqual("Expense1", viewModel.CurrentBudget.BudgetExpenseItems[0].Category);
            Assert.AreEqual(500, viewModel.CurrentBudget.BudgetExpenseItems [0].Amount.Value);

            // Edit the item
            expenseItem.BudgetCategory = "Expense";
            expenseItem.BudgetAmount = new(600);
            viewModel.ExpenseItemsSelectedIndex = 0;
            viewModel.EditExpenseItem(expenseItem);

            Assert.AreEqual(1, viewModel.CurrentBudget.BudgetExpenseItems.Count);
            Assert.AreEqual("Expense", viewModel.CurrentBudget.BudgetExpenseItems[0].Category);
            Assert.AreEqual(600, viewModel.CurrentBudget.BudgetExpenseItems[0].Amount.Value);

            // Delete the item
            viewModel.DeleteSelectedExpenseItem();
            Assert.AreEqual(0, viewModel.CurrentBudget.BudgetExpenseItems.Count);
        }

        [TestMethod]
        public void Test_CreateAndEditIncomeItem()
        {
            BudgetViewModel viewModel = new(new ContentDialogService(), new Services.MockDatabaseReader());

            NewBudgetDialogViewModel dialogViewModel = new()
            {
                SelectedDateIndex = 0
            };

            dialogViewModel.SelectedDate = dialogViewModel.AvailableBudgetDates[dialogViewModel.SelectedDateIndex];
            dialogViewModel.UseLastMonthsBudget = false;

            // Create a new budget
            viewModel.AddNewBudget(dialogViewModel);

            Assert.IsNotNull(viewModel.CurrentBudget);

            // Add a new income item
            BudgetCategoryDialogViewModel incomeItem = new()
            {
                BudgetCategory = "Income1",
                BudgetAmount = new(2000)
            };
            viewModel.CreateNewIncomeItem(incomeItem);

            Assert.AreEqual(1, viewModel.CurrentBudget.BudgetIncomeItems.Count);
            Assert.AreEqual("Income1", viewModel.CurrentBudget.BudgetIncomeItems[0].Category);
            Assert.AreEqual(2000, viewModel.CurrentBudget.BudgetIncomeItems[0].Amount.Value);

            // Edit the item
            incomeItem.BudgetCategory = "Income";
            incomeItem.BudgetAmount = new(1500);
            viewModel.IncomeItemsSelectedIndex = 0;
            viewModel.EditIncomeItem(incomeItem);

            Assert.AreEqual(1, viewModel.CurrentBudget.BudgetIncomeItems.Count);
            Assert.AreEqual("Income", viewModel.CurrentBudget.BudgetIncomeItems[0].Category);
            Assert.AreEqual(1500, viewModel.CurrentBudget.BudgetIncomeItems[0].Amount.Value);

            // Delete the item
            viewModel.DeleteSelectedIncomeItem();
            
            Assert.AreEqual(0, viewModel.CurrentBudget.BudgetIncomeItems.Count);
        }
    }
}
