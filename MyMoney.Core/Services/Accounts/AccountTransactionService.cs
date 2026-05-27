using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Accounts;

public sealed class AccountTransactionService : IAccountTransactionService
{
    private readonly IDatabaseManager _databaseManager;

    public AccountTransactionService(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public void ApplyTransactionChange(Account account, Transaction? oldTransaction, Transaction? newTransaction = null)
    {
        if (oldTransaction != null)
            account.Total -= oldTransaction.Amount;

        if (newTransaction != null)
            account.Total += newTransaction.Amount;
    }

    public (Transaction FromTransaction, Transaction ToTransaction) Transfer(
        Account fromAccount,
        Account toAccount,
        Currency amount
    )
    {
        var fromTransaction = new Transaction(
            DateTime.Today,
            "Transfer to " + toAccount.AccountName,
            new(),
            new(-amount.Value),
            "Transfer"
        )
        {
            AccountId = fromAccount.Id,
        };

        var toTransaction = new Transaction(
            DateTime.Today,
            "Transfer from " + fromAccount.AccountName,
            new(),
            amount,
            "Transfer"
        )
        {
            AccountId = toAccount.Id,
        };

        _databaseManager.Insert("Transactions", fromTransaction);
        _databaseManager.Insert("Transactions", toTransaction);

        fromAccount.Total += fromTransaction.Amount;
        toAccount.Total += toTransaction.Amount;

        return (fromTransaction, toTransaction);
    }

    public Transaction CreateBalanceUpdateTransaction(Account account, Currency newBalance, DateTime date)
    {
        var balanceChange = newBalance - account.Total;
        return new Transaction(date, "Balance update", new(), balanceChange, "Balance update")
        {
            AccountId = account.Id,
        };
    }
}
