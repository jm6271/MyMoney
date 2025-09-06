using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Reports;

public class NetWorthCalculator
{
    private readonly DatabaseManager _dbManager;

    public NetWorthCalculator(DatabaseManager dbManager)
    {
        _dbManager = dbManager;
    }

    /// <summary>
    /// Get the net worth for every day since the start date
    /// </summary>
    /// <param name="startDate">The earliest date to include in the net worth data</param>
    /// <returns>A list of currency values, with each item representing each day since the start date</returns>
    public Dictionary<DateTime, decimal> GetNetWorthSinceStartDate(DateTime startDate)
    {
        if (startDate > DateTime.Today)
        {
            throw new ArgumentException("Start date cannot be in the future.", nameof(startDate));
        }

        // Load the accounts
        var accounts = _dbManager.GetCollection<Account>("Accounts");

        // Each account has its own transactions, but we need
        // to take all of them and put them in a single list, sorted by date from oldest to newest
        var allTransactions = accounts
            .SelectMany(account => account.Transactions)
            .Where(transaction => transaction.Date >= startDate)
            .OrderBy(transaction => transaction.Date)
            .ToList();

        // Since we have no way of knowing the total balance of the accounts at the start date,
        // we will have to start with the most recent transaction and work backwards
        decimal currentNetWorth = 0m;
        foreach (var account in accounts)
        {
            currentNetWorth += account.Total.Value;
        }

        Dictionary<DateTime, decimal> netWorthData = [];
        netWorthData.Add(DateTime.Today, currentNetWorth);

        DateTime currentDate = DateTime.Today;
        while(currentDate > startDate)
        {
            // Add all transactions for the current date
            var transactionsForDate = allTransactions.Where(t => t.Date.Date == currentDate.Date);
            foreach (var transaction in transactionsForDate)
            {
                currentNetWorth -= transaction.Amount.Value;
            }
            // Add the current net worth to the list
            netWorthData.Add(currentDate.AddDays(-1), currentNetWorth);
            // Move to the previous day
            currentDate = currentDate.AddDays(-1);
        }

        // Sort dictionary by date, oldest to newest
        netWorthData = netWorthData.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return netWorthData;
    }
}
