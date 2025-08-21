using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Reports
{
    public static class BudgetReportCalculator
    {
        /// <summary>
        /// Calculate an income items report for all income in the specified month
        /// </summary>
        /// <param name="budgetMonth">The month of the budget to generate a report on</param>
        /// <param name="databaseReader">The database reader to use to get the information for the report</param>
        /// <returns>A list of income report items that occurred in the specified month</returns>
        public static List<BudgetReportItem> CalculateIncomeReportItems(DateTime budgetMonth, IDatabaseManager databaseReader)
        {
            // read the budget from the database
            BudgetCollection budgetCollection = new(databaseReader);

            // Make sure the specified budget exists
            var budget = budgetCollection.Budgets.FirstOrDefault(b => b.BudgetDate.Month == budgetMonth.Month && b.BudgetDate.Year == budgetMonth.Year);

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
                    Actual = CalculateTotalForCategory(new() { Group = "Income", Name = item.Category }, budgetMonth, databaseReader),
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
        public static List<BudgetReportItem> CalculateIncomeReportItems(IDatabaseManager databaseReader)
        {
            return CalculateIncomeReportItems(DateTime.Today, databaseReader);
        }

        /// <summary>
        /// Calculate the savings category items for the specified month
        /// </summary>
        /// <param name="budgetMonth">The month of the budget to generate a report on</param>
        /// <param name="databaseReader">The database reader to use to get the information for the report</param>
        /// <returns>A list of report items, for each savings category</returns>
        public static List<SavingsCategoryReportItem> CalculateSavingsReportItems(DateTime budgetMonth, IDatabaseManager databaseReader)
        {
            // read the budget from the database
            BudgetCollection budgetCollection = new(databaseReader);

            // Make sure the specified budget exists
            var budget = budgetCollection.Budgets.FirstOrDefault(b => b.BudgetDate.Month == budgetMonth.Month && b.BudgetDate.Year == budgetMonth.Year);

            if (budget == null)
            {
                // No budget for this month
                return [];
            }

            var savingsItems = budget.BudgetSavingsCategories;

            // Create a list of budget report items
            List<SavingsCategoryReportItem> budgetReportItems = [];

            // loop through the incomeItems and create the budget report items
            foreach (var item in savingsItems)
            {
                SavingsCategoryReportItem budgetReportItem = new()
                {
                    Category = item.CategoryName,
                    Saved = item.BudgetedAmount,
                    Spent = new(-CalculateTotalForCategory(new() { Group = "Savings", Name = item.CategoryName }, budgetMonth, databaseReader).Value),
                    Balance = item.CurrentBalance,
                };

                budgetReportItems.Add(budgetReportItem);
            }

            return budgetReportItems;
        }

        /// <summary>
        /// Calculate an expense items report for all expenses in the specified month
        /// </summary>
        /// <param name="budgetMonth">The month of the budget to generate a report on</param>
        /// <param name="databaseReader">The database reader to use to get the information for the report</param>
        /// <returns>A list of expense report items that occurred in the specified month</returns>
        public static List<BudgetReportItem> CalculateExpenseReportItems(DateTime budgetMonth, IDatabaseManager databaseReader)
        {
            // read the budget from the database
            BudgetCollection budgetCollection = new(databaseReader);

            // Make sure the specified budget exists
            var budget = budgetCollection.Budgets.FirstOrDefault(b => b.BudgetDate.Month == budgetMonth.Month && b.BudgetDate.Year == budgetMonth.Year);

            if (budget == null)
            {
                // No budget for this month
                return [];
            }

            var expenseItems = budget.BudgetExpenseItems;

            // Create a list of budget report items
            List<BudgetReportItem> budgetReportItems = [];

            // loop through the expense items and create the budget report items
            foreach (var item in expenseItems)
            {
                foreach (var subItem in item.SubItems)
                {

                    BudgetReportItem budgetReportItem = new()
                    {
                        Category = subItem.Category,
                        Group = item.CategoryName,
                        Budgeted = subItem.Amount,
                        Actual = new(Math.Abs(CalculateTotalForCategory(new() { Group = item.CategoryName, Name = subItem.Category}, budgetMonth, databaseReader).Value)),
                    };

                    budgetReportItem.Remaining = budgetReportItem.Budgeted - budgetReportItem.Actual;

                    budgetReportItems.Add(budgetReportItem);
                }
            }

            return budgetReportItems;
        }

        /// <summary>
        /// Calculate the expense report items from the beginning of the current month
        /// </summary>
        /// <returns>A list of report items, for each budget category</returns>
        public static List<BudgetReportItem> CalculateExpenseReportItems(IDatabaseManager databaseReader)
        {
            return CalculateExpenseReportItems(DateTime.Today, databaseReader);
        }

        /// <summary>
        /// Calculates all the items for a budget report, or uses a cached version if available
        /// </summary>
        /// <param name="date">The date of the report</param>
        /// <returns>The income, expenses, and savings report items</returns>
        public static (List<BudgetReportItem> income, List<BudgetReportItem> expenses, List<SavingsCategoryReportItem> savings)
            CalculateBudgetReport(DateTime date, IDatabaseManager databaseManager)
        {
            // Check to see if data is cached
            ReportsCache cache = new(databaseManager);
            string key = ReportsCache.GenerateKeyForBudgetReportCache(date);
            if (cache.DoesKeyExist(key))
            {
                // Read cache data
                Dictionary<string, object> cachedReport;
                cache.RetrieveCachedObject(key, out object? cachedItem);
                if (cachedItem != null)
                {
                    try
                    {
                        cachedReport = (Dictionary<string, object>)cachedItem;

                        // Ensure the correct items are in the cached report
                        if (cachedReport.TryGetValue("Income", out object? cachedIncomeObject) &&
                            cachedReport.TryGetValue("Expenses", out object? cachedExpensesObject) &&
                            cachedReport.TryGetValue("Savings", out object? cachedSavingsObject))
                        {
                            // convert to arrays of object
                            var cachedIncomeObjectArray = (object[])cachedIncomeObject;
                            var cachedExpenseObjectArray = (object[])cachedExpensesObject;
                            var cachedSavingsObjectArray = (object[])cachedSavingsObject;

                            // Load the cached data and return it
                            List<BudgetReportItem> cachedIncome = [];
                            foreach (var item in cachedIncomeObjectArray)
                            {
                                cachedIncome.Add((BudgetReportItem)item);
                            }

                            List<BudgetReportItem> cachedExpenses = [];
                            foreach (var item in cachedExpenseObjectArray)
                            {
                                cachedExpenses.Add((BudgetReportItem)item);
                            }

                            List<SavingsCategoryReportItem> cachedSavings = [];
                            foreach (var item in cachedSavingsObjectArray)
                            {
                                cachedSavings.Add((SavingsCategoryReportItem)item);
                            }

                            if (cachedIncome.Count > 0 && cachedExpenses.Count > 0 && cachedSavings.Count > 0)
                                return (cachedIncome, cachedExpenses, cachedSavings);
                        }
                    }
                    catch (Exception)
                    {
                        // Cache is invalid, calculate report from scratch
                        cache.UncacheObject(key);
                    }
                }
            }

            // Load data and cache it
            var calculatedIncomeItems = CalculateIncomeReportItems(date, databaseManager);
            var calculatedExpenseItems = CalculateExpenseReportItems(date, databaseManager);
            var calulatedSavingsItems = CalculateSavingsReportItems(date, databaseManager);

            Dictionary<string, object> reportToCache = [];
            reportToCache["Income"] = calculatedIncomeItems;
            reportToCache["Expenses"] = calculatedExpenseItems;
            reportToCache["Savings"] = calulatedSavingsItems;

            cache.CacheObject(key, reportToCache);

            return (
                calculatedIncomeItems,
                calculatedExpenseItems,
                calulatedSavingsItems
            );
        }

        private static Currency CalculateTotalForCategory(Category category, DateTime month, IDatabaseManager databaseReader)
        {
            // Read the accounts from the database
            var accounts = databaseReader.GetCollection<Account>("Accounts");

            // search through each account for transactions in this category that happened in the specified month
            var actual = 0m;
            foreach (var account in accounts)
            {
                foreach (var transaction in account.Transactions)
                {
                    if (transaction.Category.Name == category.Name && IsDateInCurrentMonth(transaction.Date, month))
                    {
                        actual += transaction.Amount.Value;
                    }
                }
            }

            return new Currency(actual);
        }

        private static bool IsDateInCurrentMonth(DateTime date, DateTime month)
        {
            return date.Year == month.Year && date.Month == month.Month;
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
