using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Accounts;

public interface ITransactionQueryService
{
    Task<IReadOnlyList<Transaction>> GetTransactionsPage(TransactionPageRequest request);
}
