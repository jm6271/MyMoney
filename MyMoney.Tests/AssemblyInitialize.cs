#define TESTING

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMoney.Core.Database;
using MyMoney.Core.FS.Models;
using MyMoney.ViewModels.Pages;

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


            // Create an account
            Account account = new();
            account.AccountName = "Account";
            account.Transactions.Add(new Transaction(DateTime.Today, "Begining Balance", string.Empty, new(0.00m), new(0.0m), new(5000), string.Empty));
            account.Total = new(2000);

            // Add some transactions
            account.Transactions.Add(new Transaction(DateTime.Today, "My Job, Inc.", "Income", new(0m), new(500), new(2500), "Paycheck"));
            account.Transactions.Add(new Transaction(DateTime.Today, "Gas Station", "Auto", new(50m), new(0m), new(2450m), "Fill up car"));
            account.Transactions.Add(new Transaction(DateTime.Today, "Fast Food, Inc.", "Food", new(20m), new(0m), new(2430), "Lunch"));
            account.Total = new(2430);

            // Write account to the database
            List<Account> accounts = [account];
            DatabaseWriter.WriteCollection("Accounts", accounts);
        }
    }
}
