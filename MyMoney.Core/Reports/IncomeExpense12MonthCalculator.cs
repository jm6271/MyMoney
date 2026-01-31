using System.Globalization;
using System.Threading.Tasks;
using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Reports
{
    /// <summary>
    /// Calculates values for a 12-month income vs. expense bar chart
    /// </summary>
    public static class IncomeExpense12MonthCalculator
    {
        /// <summary>
        /// Get the names of all the months from 11 months ago to the current month
        /// </summary>
        /// <returns>A list of month names</returns>
        public static List<string> GetMonthNames()
        {
            List<string> monthNames = [];
            var currentDate = DateTime.Now;

            for (var i = 11; i >= 0; i--)
            {
                var targetDate = currentDate.AddMonths(-i);
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(targetDate.Month);
                monthNames.Add(monthName);
            }

            return monthNames;
        }

        public static async Task<List<double>> GetPast12MonthsIncome(IDatabaseManager dbManager)
        {
            List<double> income = [];

            for (var i = 11; i >= 0; i--)
            {
                var transactions = await GetMonthOfTransactions(dbManager, i);

                var total = GetIncome(transactions);
                income.Add((double)total);
            }

            return income;
        }

        public static async Task<List<double>> GetPast12MonthsExpenses(IDatabaseManager dbManager)
        {
            List<double> expenses = [];

            for (var i = 11; i >= 0; i--)
            {
                var transactions = await GetMonthOfTransactions(dbManager, i);

                var total = GetExpenses(transactions);
                expenses.Add((double)total);
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

            await dbReader.ExecuteAsync(async db =>
            {
                var transactionCollection = db.GetCollection<Transaction>("Transactions");
                transactions.AddRange(transactionCollection.Query().Where(t => (t.Date >= startDate && t.Date <= endDate)).ToList());
            });

            return transactions;
        }
    }
}
