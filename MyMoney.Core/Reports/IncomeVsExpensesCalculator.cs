using System.Globalization;
using System.Threading.Tasks;
using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Reports
{
    /// <summary>
    /// Calculates values for a multiple month income vs. expense bar chart
    /// </summary>
    public static class IncomeVsExpensesCalculator
    {
        /// <summary>
        /// Get the names of all the months from the specified number to the current month.
        /// </summary>
        /// <returns>A list of month names</returns>
        public static List<string> GetMonthNames(int numMonths = 12)
        {
            List<string> monthNames = [];
            var currentDate = DateTime.Now;

            for (var i = numMonths - 1; i >= 0; i--)
            {
                var targetDate = currentDate.AddMonths(-i);
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(targetDate.Month);
                monthNames.Add(monthName);
            }

            return monthNames;
        }

        /// <summary>
        /// Get the years from the specified number of years ago through the current year.
        /// </summary>
        public static List<string> GetYearNames(int numYears = 11)
        {
            var currentYear = DateTime.Now.Year;
            return Enumerable.Range(currentYear - numYears + 1, numYears).Select(year => year.ToString()).ToList();
        }

        public static async Task<List<double>> GetPastMonthsIncome(IDatabaseManager dbManager, int numMonths = 12)
        {
            List<double> income = [];

            for (var i = numMonths - 1; i >= 0; i--)
            {
                var transactions = await GetMonthOfTransactions(dbManager, i);

                var total = GetIncome(transactions);
                income.Add((double)total);
            }

            return income;
        }

        public static async Task<List<double>> GetPastMonthsExpenses(IDatabaseManager dbManager, int numMonths = 12)
        {
            List<double> expenses = [];

            for (var i = numMonths - 1; i >= 0; i--)
            {
                var transactions = await GetMonthOfTransactions(dbManager, i);

                var total = GetExpenses(transactions);
                expenses.Add((double)total);
            }

            return expenses;
        }

        public static async Task<List<double>> GetPastYearsIncome(IDatabaseManager dbManager, int numYears = 11)
        {
            List<double> income = [];

            for (var i = numYears - 1; i >= 0; i--)
            {
                var transactions = await GetYearOfTransactions(dbManager, i);
                income.Add((double)GetIncome(transactions));
            }

            return income;
        }

        public static async Task<List<double>> GetPastYearsExpenses(IDatabaseManager dbManager, int numYears = 11)
        {
            List<double> expenses = [];

            for (var i = numYears - 1; i >= 0; i--)
            {
                var transactions = await GetYearOfTransactions(dbManager, i);
                expenses.Add((double)GetExpenses(transactions));
            }

            return expenses;
        }

        private static decimal GetIncome(List<Transaction> transactions)
        {
            return (
                from transaction in transactions
                where !string.IsNullOrWhiteSpace(transaction.Category.Name)
                where transaction.Amount.Value > 0
                select transaction.Amount.Value
            ).Sum();
        }

        private static decimal GetExpenses(List<Transaction> transactions)
        {
            var expenses = (
                from transaction in transactions
                where !string.IsNullOrWhiteSpace(transaction.Category.Name)
                where transaction.Amount.Value < 0
                select transaction.Amount.Value
            ).Sum();

            return Math.Abs(expenses);
        }

        private static async Task<List<Transaction>> GetMonthOfTransactions(IDatabaseManager dbManager, int monthsAgo)
        {
            var startDate = GetMonthStartDate(monthsAgo);
            var endDate = GetMonthEndDate(monthsAgo);

            var result = await ReadTransactionsWithingDateRange(dbManager, startDate, endDate);

            return result;
        }

        private static async Task<List<Transaction>> GetYearOfTransactions(IDatabaseManager dbManager, int yearsAgo)
        {
            var year = DateTime.Now.Year - yearsAgo;
            return await ReadTransactionsWithingDateRange(
                dbManager,
                new DateTime(year, 1, 1),
                new DateTime(year, 12, 31)
            );
        }

        private static DateTime GetMonthEndDate(int monthsAgo)
        {
            var monthNumber = DateTime.Now.Month;
            var yearNumber = DateTime.Now.Year;

            if (monthsAgo < monthNumber)
                monthNumber -= monthsAgo;
            else
            {
                monthNumber = 12 + monthNumber - monthsAgo;
                yearNumber--;
            }

            var day = DateTime.DaysInMonth(yearNumber, monthNumber);
            DateTime dt = new(yearNumber, monthNumber, day);
            return dt;
        }

        private static DateTime GetMonthStartDate(int monthsAgo)
        {
            var monthNumber = DateTime.Now.Month;
            var yearNumber = DateTime.Now.Year;

            if (monthsAgo < monthNumber)
                monthNumber -= monthsAgo;
            else
            {
                monthNumber = 12 + monthNumber - monthsAgo;
                yearNumber--;
            }
            DateTime dt = new(yearNumber, monthNumber, 1);
            return dt;
        }

        private static async Task<List<Transaction>> ReadTransactionsWithingDateRange(IDatabaseManager dbReader, DateTime startDate, DateTime endDate)
        {
            List<Transaction> transactions = [];

            await dbReader.QueryAsync<Transaction>("Transactions", async query =>
            {
                transactions.AddRange(
                    query.Where(t => t.AccountId != -1 && t.Date >= startDate && t.Date <= endDate).ToList()
                );
            });

            return transactions;
        }
    }
}
