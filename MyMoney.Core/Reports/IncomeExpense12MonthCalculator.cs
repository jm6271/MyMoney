using MyMoney.Core.Database;
using MyMoney.Core.FS.Models;
using System.Globalization;
using System.Linq;

namespace MyMoney.Core.Reports
{
    /// <summary>
    /// Calculates values for a 12 month income vs. expense bar chart
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
            DateTime currentDate = DateTime.Now; 
            
            for (int i = 11; i >= 0; i--) 
            { 
                DateTime targetDate = currentDate.AddMonths(-i); 
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(targetDate.Month); 
                monthNames.Add(monthName);
            } 
            
            return monthNames; 
        }

        public static List<double> GetPast12MonthsIncome()
        {
            List<double> income = [];

            for (int i = 11; i >= 0; i--)
            {
                var transactions = GetMonthOfTransactions(i);

                var total = GetIncome(transactions);
                income.Add((double)total);
            }

            return income;
        }

        public static List<double> GetPast12MonthsExpenses()
        {
            List<double> expenses = [];

            for (int i = 11; i >= 0; i--)
            {
                var transactions = GetMonthOfTransactions(i);

                var total = GetExpenses(transactions);
                expenses.Add((double)total);
            }

            return expenses;
        }

        private static decimal GetIncome(List<Transaction> transactions)
        {
            decimal income = 0;

            foreach (Transaction transaction in transactions)
            {
                // Only list items with a category (this prevents beginning balances and transfers from showing up in the chart)
                if (string.IsNullOrWhiteSpace(transaction.Category))
                    continue;

                if (transaction.Amount.Value > 0)
                    income += transaction.Amount.Value;
            }

            return income;
        }

        private static decimal GetExpenses(List<Transaction> transactions)
        {
            decimal expenses = 0;

            foreach (Transaction transaction in transactions)
            {
                // Only list items with a category (this prevents transfers from showing up in the chart)
                if (string.IsNullOrWhiteSpace(transaction.Category))
                    continue;
                
                if (transaction.Amount.Value < 0)
                    expenses += transaction.Amount.Value;
            }

            return Math.Abs(expenses);
        }

        private static List<Transaction> GetMonthOfTransactions(int monthsAgo)
        {
            var startDate = GetMonthStartDate(monthsAgo);
            var endDate = GetMonthEndDate(monthsAgo);

            var result = ReadTransactionsWithingDateRange(startDate, endDate);

            return result;
        }

        private static DateTime GetMonthEndDate(int monthsAgo)
        {
            int monthNumber = DateTime.Now.Month;
            int yearNumber = DateTime.Now.Year;

            if (monthsAgo < monthNumber)
                monthNumber -= monthsAgo;
            else
            {
                monthNumber = 12 + monthNumber - monthsAgo;
                yearNumber--;
            }

            int day = DateTime.DaysInMonth(yearNumber, monthNumber);
            DateTime dt = new(yearNumber, monthNumber, day);
            return dt;
        }

        private static DateTime GetMonthStartDate(int monthsAgo)
        {
            int monthNumber = DateTime.Now.Month;
            int yearNumber = DateTime.Now.Year;

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

        private static List<Transaction> ReadTransactionsWithingDateRange(DateTime startDate, DateTime endDate)
        {
            // Load list of transactions from the database
            var accounts = DatabaseReader.GetCollection<Account>("Accounts");

            List<Transaction> allTransactions = [];

            foreach (var account in accounts)
            {
                foreach (var t in account.Transactions)
                {
                    allTransactions.Add(t);
                }
            }

            // go through the transactions and get the ones in the specified date range
            List<Transaction> transactions = [];
            transactions.AddRange(from transaction in allTransactions
                                  where IsDateBetween(transaction.Date, startDate, endDate)
                                  select transaction);
            return transactions;
        }

        public static bool IsDateBetween(DateTime dateToCheck, DateTime startDate, DateTime endDate) 
        { 
            return dateToCheck >= startDate && dateToCheck <= endDate; 
        }
    }
}
