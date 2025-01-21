using MyMoney.Core.Database;
using MyMoney.Core.FS.Models;

namespace MyMoney.Core.Reports
{
    public static class BudgetReportCalculator
    {
        public static List<BudgetReportItem> CalculateIncomeReportItems()
        {
            // read income items collection from the database
            BudgetCollection budgetCollection = new();
            if (!budgetCollection.DoesCurrentBudgetExist())
            {
                return [];
            }

            var incomeItems = budgetCollection.GetCurrentBudget().BudgetIncomeItems;

            // Create a list of budget report items
            List<BudgetReportItem> budgetReportItems = [];

            // loop through the incomeItems and create the budget report items
            foreach (var item in incomeItems)
            {
                BudgetReportItem budgetReportItem = new()
                {
                    Category = item.Category,
                    Budgeted = item.Amount,
                    Actual = CalculateTotalForCategory(item.Category),
                };

                budgetReportItem.Remaining = new(
                    InvertSign((budgetReportItem.Budgeted - budgetReportItem.Actual).Value));

                budgetReportItems.Add(budgetReportItem);
            }

            return budgetReportItems;
        }

        public static List<BudgetReportItem> CalculateExpenseReportItems()
        {
            BudgetCollection budgetCollection = new();
            if (!budgetCollection.DoesCurrentBudgetExist())
            {
                return [];
            }

            var expenseItems = budgetCollection.GetCurrentBudget().BudgetExpenseItems;

            // Create a list of budget report items
            List<BudgetReportItem> budgetReportItems = [];

            // loop through the expenseItems and create the budget report items
            foreach (var item in expenseItems)
            {
                BudgetReportItem budgetReportItem = new()
                {
                    Category = item.Category,
                    Budgeted = item.Amount,
                    Actual = new(Math.Abs(CalculateTotalForCategory(item.Category).Value)),
                };

                budgetReportItem.Remaining = budgetReportItem.Budgeted - budgetReportItem.Actual;

                budgetReportItems.Add(budgetReportItem);
            }

            return budgetReportItems;
        }

        private static Currency CalculateTotalForCategory(string CategoryName)
        {
            // Read the accounts from the database
            var accounts = Database.DatabaseReader.GetCollection<Account>("Accounts");

            // search through each account for transactions in this category that happened this month

            // Total for this category so far
            decimal Actual = 0m;

            foreach (var account in accounts)
            {
                foreach (var transaction in account.Transactions)
                {
                    if (transaction.Category == CategoryName && IsDateInCurrentMonth(transaction.Date))
                    {
                        Actual += transaction.Amount.Value;
                    }
                }
            }

            return new Currency(Actual);
        }

        private static bool IsDateInCurrentMonth(DateTime date)
        {
            DateTime currentDate = DateTime.Now;
            return date.Year == currentDate.Year && date.Month == currentDate.Month;
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
