using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Accounts;

public interface IAccountTransactionService
{
    void ApplyTransactionChange(Account account, Transaction? oldTransaction, Transaction? newTransaction = null);
    (Transaction FromTransaction, Transaction ToTransaction) Transfer(Account fromAccount, Account toAccount, Currency amount);
    Transaction CreateBalanceUpdateTransaction(Account account, Currency newBalance, DateTime date);
}
