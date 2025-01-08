#define TESTING

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMoney.Core.Database;
using MyMoney.Core.FS.Models;

namespace MyMoney.Tests
{
    [TestClass]
    public static class AssemblyInitialize
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            // Delete the test database if it exists
            if (System.IO.File.Exists(DataFileLocationGetter.GetDataFilePath()))
            {
                System.IO.File.Delete(DataFileLocationGetter.GetDataFilePath());
            }

            // Write some data to the test database for the unit tests to use

            // Create a budget for the current month
            Budget budget = new();
            budget.BudgetDate = DateTime.Now;
            budget.BudgetTitle = BudgetCollection.GetCurrentBudgetName();
            budget.BudgetIncomeItems.Add(new BudgetItem { Category = "Income", Amount = new Currency(2000) });
            budget.BudgetIncomeItems.Add(new BudgetItem { Category = "Interest", Amount = new Currency(20) });
            budget.BudgetExpenseItems.Add(new BudgetItem { Category = "Housing", Amount = new Currency(800) });
            budget.BudgetExpenseItems.Add(new BudgetItem { Category = "Charity", Amount = new Currency(200) });
            budget.BudgetExpenseItems.Add(new BudgetItem { Category = "Auto", Amount = new Currency(250) });
            budget.BudgetExpenseItems.Add(new BudgetItem { Category = "Food", Amount = new Currency(300) });
            budget.BudgetExpenseItems.Add(new BudgetItem { Category = "Savings", Amount = new Currency(470) });

            // Write budget to database
            List<Budget> budgets = [budget];
            DatabaseWriter.WriteCollection("Budgets", budgets);

        }
    }
}
