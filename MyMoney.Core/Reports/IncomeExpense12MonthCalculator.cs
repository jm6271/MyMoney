using MyMoney.Core.Database;
using MyMoney.Core.Models;
using System.Globalization;

namespace MyMoney.Core.Reports
{
    /// <summary>
    /// Calculates values for a 12 month income vs. expense bar chart
    /// </summary>
    public class IncomeExpense12MonthCalculator
    {
        /// <summary>
        /// Get a list of income and expenses for the last 12 months
        /// </summary>
        /// <returns>A list that has values alternating between income and expenses</returns>
        public static List<double> GetIncomeAndExpenses()
        {
            List<double> result = [];

            var income = GetPast12MonthsIncome();
            var expenses = GetPast12MonthsExpenses();

            for (int i = 0; i < income.Count; i++)
            {
                result.Add((double)income[i]);
                result.Add((double)expenses[i]);
                result.Add(double.NaN); // blank value between each month
            }

            return result;
        }

        /// <summary>
        /// Get the names of all the months from 11 months ago to the current month
        /// </summary>
        /// <returns>A list of month names</returns>
        public static List<string> GetMonthNames() 
        { 
            List<string> monthNames = new List<string>(); 
            DateTime currentDate = DateTime.Now; 
            
            for (int i = 11; i >= 0; i--) 
            { 
                DateTime targetDate = currentDate.AddMonths(-i); 
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(targetDate.Month); 
                monthNames.Add(monthName);

                // Insert an empty string every other one because each month has two bars
                monthNames.Add("");
            } 
            
            return monthNames; 
        }

        /// <summary>
        /// Get the position for each label on the chart
        /// </summary>
        /// <returns>A list of positions</returns>
        public static List<double> GetLabelPosistions()
        {
            List<double> positions = [
                0.5,
                3.5,
                6.5,
                9.5,
                12.5,
                15.5,
                18.5,
                21.5,
                24.5,
                27.5,
                30.5,
                33.5,
                36.5
                ];
            return positions;
        }

        private static List<decimal> GetPast12MonthsIncome()
        {
            List<decimal> income = [];

            for (int i = 11; i >= 0; i--)
            {
                var transactions = GetMonthOfTransactions(i);

                var total = GetIncome(transactions);
                income.Add(total);
            }

            return income;
        }

        private static List<decimal> GetPast12MonthsExpenses()
        {
            List<decimal> expenses = [];

            for (int i = 11; i >= 0; i--)
            {
                var transactions = GetMonthOfTransactions(i);

                var total = GetExpenses(transactions);
                expenses.Add(total);
            }

            return expenses;
        }

        private static decimal GetIncome(List<Transaction> transactions)
        {
            decimal income = 0;

            foreach (Transaction transaction in transactions)
            {
                income += transaction.Receive.Value;
            }

            return income;
        }

        private static decimal GetExpenses(List<Transaction> transactions)
        {
            decimal expenses = 0;

            foreach (Transaction transaction in transactions)
            {
                expenses += transaction.Spend.Value;
            }

            return expenses;
        }

        private static List<Transaction> GetMonthOfTransactions(int monthsAgo)
        {
            List<Transaction> result = [];
            var startDate = GetMonthStartDate(monthsAgo);
            var endDate = GetMonthEndDate(monthsAgo);

            result = ReadTransactionsWithingDateRange(startDate, endDate);

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
            DateTime dt = new(yearNumber, monthNumber, DateTime.DaysInMonth(yearNumber, monthNumber));
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

            foreach (var transaction in allTransactions)
            {
                if (IsDateBetween(transaction.Date, startDate, endDate))
                {
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        public static bool IsDateBetween(DateTime dateToCheck, DateTime startDate, DateTime endDate) 
        { 
            return dateToCheck >= startDate && dateToCheck <= endDate; 
        }
    }
}
