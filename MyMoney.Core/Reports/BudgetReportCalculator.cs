using MyMoney.Core.Database;
using MyMoney.Core.FS.Models;

namespace MyMoney.Core.Reports
{
    public static class BudgetReportCalculator
    {
        /// <summary>
        /// Calculate an income items report for all income in the specified month
        /// </summary>
        /// <param name="BudgetMonth">The month of the budget to generate a report on</param>
        /// <returns>A list of income report items that occurred in the specified month</returns>
        public static List<BudgetReportItem> CalculateIncomeReportItems(DateTime BudgetMonth, IDatabaseReader databaseReader)
        {
            // read the budget from the database
            BudgetCollection budgetCollection = new(databaseReader);

            // Make sure the specified budget exists
            Budget? budget = null;

            foreach (var b in budgetCollection.Budgets)
            {
                if (b.BudgetDate.Month == BudgetMonth.Month &&
                    b.BudgetDate.Year == BudgetMonth.Year)
                {
                    budget = b;
                    break;
                }
            }

            if (budget == null)
            {
                // No budget for this month
                return [];
            }

            var incomeItems = budget.BudgetIncomeItems;

            // Create a list of budget report items
            List<BudgetReportItem> budgetReportItems = [];

            // loop through the incomeItems and create the budget report items
            foreach (var item in incomeItems)
            {
                BudgetReportItem budgetReportItem = new()
                {
                    Category = item.Category,
                    Budgeted = item.Amount,
                    Actual = CalculateTotalForCategory(item.Category, BudgetMonth, databaseReader),
                };

                budgetReportItem.Remaining = new(
                    InvertSign((budgetReportItem.Budgeted - budgetReportItem.Actual).Value));

                budgetReportItems.Add(budgetReportItem);
            }

            return budgetReportItems;
        }

        /// <summary>
        /// Calculate the income report items from the beginning of the current month
        /// </summary>
        /// <returns>A list of report items, for each budget category</returns>
        public static List<BudgetReportItem> CalculateIncomeReportItems(IDatabaseReader databaseReader)
        {
            return CalculateIncomeReportItems(DateTime.Today, databaseReader);
        }

        /// <summary>
        /// Calculate an expense items report for all expenses in the specified month
        /// </summary>
        /// <param name="BudgetMonth">The month of the budget to generate a report on</param>
        /// <returns>A list of expense report items that occurred in the specified month</returns>
        public static List<BudgetReportItem> CalculateExpenseReportItems(DateTime BudgetMonth, IDatabaseReader databaseReader)
        {
            // read the budget from the database
            BudgetCollection budgetCollection = new(databaseReader);

            // Make sure the specified budget exists
            Budget? budget = null;

            foreach (var b in budgetCollection.Budgets)
            {
                if (b.BudgetDate.Month == BudgetMonth.Month &&
                    b.BudgetDate.Year == BudgetMonth.Year)
                {
                    budget = b;
                    break;
                }
            }

            if (budget == null)
            {
                // No budget for this month
                return [];
            }

            var expenseItems = budget.BudgetExpenseItems;

            // Create a list of budget report items
            List<BudgetReportItem> budgetReportItems = [];

            // loop through the incomeItems and create the budget report items
            foreach (var item in expenseItems)
            {
                BudgetReportItem budgetReportItem = new()
                {
                    Category = item.Category,
                    Budgeted = item.Amount,
                    Actual = new(Math.Abs(CalculateTotalForCategory(item.Category, BudgetMonth, databaseReader).Value)),
                };

                budgetReportItem.Remaining = budgetReportItem.Budgeted - budgetReportItem.Actual;

                budgetReportItems.Add(budgetReportItem);
            }

            return budgetReportItems;
        }

        /// <summary>
        /// Calculate the expense report items from the beginning of the current month
        /// </summary>
        /// <returns>A list of report items, for each budget category</returns>
        public static List<BudgetReportItem> CalculateExpenseReportItems(IDatabaseReader databaseReader)
        {
            return CalculateExpenseReportItems(DateTime.Today, databaseReader);
        }

        private static Currency CalculateTotalForCategory(string CategoryName, DateTime Month, IDatabaseReader databaseReader)
        {
            // Read the accounts from the database
            var accounts = databaseReader.GetCollection<Account>("Accounts");

            // search through each account for transactions in this category that happened in the specified month

            // Total for this category so far
            decimal Actual = 0m;

            foreach (var account in accounts)
            {
                foreach (var transaction in account.Transactions)
                {
                    if (transaction.Category == CategoryName && IsDateInCurrentMonth(transaction.Date, Month))
                    {
                        Actual += transaction.Amount.Value;
                    }
                }
            }

            return new Currency(Actual);
        }

        private static bool IsDateInCurrentMonth(DateTime date, DateTime Month)
        {
            return date.Year == Month.Year && date.Month == Month.Month;
        }

        private static decimal InvertSign(decimal amount)
        {
            if (amount < 0)
                return Math.Abs(amount);
            var temp = amount;
            amount -= temp * 2;
            return amount;
        }
    }
}
