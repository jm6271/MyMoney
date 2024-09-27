using MyMoney.Models;
using MyMoney.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Tests
{
    [TestClass]
    public class BudgetViewModelTests
    {
        [TestMethod]
        public void TestUpdateListViewTotals()
        {
            BudgetViewModel viewModel = new();

            // clear the income and expense line items
            viewModel.ExpenseLineItems.Clear();
            viewModel.IncomeLineItems.Clear();

            // add some income line items
            BudgetIncomeItem incomeItem1 = new();
            incomeItem1.Category = "Work";
            incomeItem1.Amount = new(1000m);
            viewModel.IncomeLineItems.Add(incomeItem1);

            BudgetIncomeItem incomeItem2 = new();
            incomeItem2.Category = "Side Jobs";
            incomeItem2.Amount = new(250m);
            viewModel.IncomeLineItems.Add(incomeItem2);

            // Add some expense line items
            BudgetExpenseItem expenseItem1 = new();
            expenseItem1.Category = "Gas";
            expenseItem1.Amount = new(120m);
            viewModel.ExpenseLineItems.Add(expenseItem1);

            BudgetExpenseItem expenseItem2 = new();
            expenseItem2.Category = "Misc.";
            expenseItem2.Amount = new(150m);
            viewModel.ExpenseLineItems.Add(expenseItem2);

            // Call the update totals method
            viewModel.UpdateListViewTotals();


            // make some asserts
            Assert.AreEqual(1250m, viewModel.IncomeTotal.Value, "#2");

            Assert.AreEqual(270m, viewModel.ExpenseTotal.Value, "#2");
        }
    }
}
