using MyMoney.Core.Models;

namespace MyMoney.Core.Reports
{
    public class BudgetReportCalculator
    {
        public static List<BudgetReportItem> CalculateIncomeReportItems()
        {
            // read income items collection from the database
            var incomeItems = Database.DatabaseReader.GetCollection<BudgetIncomeItem>("BudgetIncomeItems");

            // Create a list of budget report items
            List<BudgetReportItem> budgetReportItems = [];

            // loop through the incomeItems and create the budget report items
            foreach (var item in incomeItems)
            {
                BudgetReportItem budgetReportItem = new()
                {
                    Category = item.Category,
                    Budgeted = item.Amount,
                    Actual = CalculateTotalIncomeForCategory(item.Category),
                };

                budgetReportItem.Remaining = new(
                    InvertSign((budgetReportItem.Budgeted - budgetReportItem.Actual).Value));

                budgetReportItems.Add(budgetReportItem);
            }

            return budgetReportItems;
        }

        public static List<BudgetReportItem> CalculateExpenseReportItems()
        {
            // read expense items collection from the database
            var expenseItems = Database.DatabaseReader.GetCollection<BudgetIncomeItem>("BudgetExpenseItems");

            // Create a list of budget report items
            List<BudgetReportItem> budgetReportItems = [];

            // loop through the expenseItems and create the budget report items
            foreach (var item in expenseItems)
            {
                BudgetReportItem budgetReportItem = new()
                {
                    Category = item.Category,
                    Budgeted = item.Amount,
                    Actual = CalculateTotalExpensesForCategory(item.Category),
                };

                budgetReportItem.Remaining = budgetReportItem.Budgeted - budgetReportItem.Actual;

                budgetReportItems.Add(budgetReportItem);
            }

            return budgetReportItems;
        }

        private static Currency CalculateTotalIncomeForCategory(string CategoryName)
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
                        Actual += transaction.Receive.Value;
                    }
                }
            }

            return new Currency(Actual);
        }

        private static Currency CalculateTotalExpensesForCategory(string CategoryName)
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
                        Actual += transaction.Spend.Value;
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
