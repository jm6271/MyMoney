using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Accounts;

public sealed class TransactionQueryService : ITransactionQueryService
{
    private readonly IDatabaseManager _databaseManager;

    public TransactionQueryService(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public async Task<IReadOnlyList<Transaction>> GetTransactionsPage(TransactionPageRequest request)
    {
        List<Transaction> transactions = [];

        await _databaseManager.QueryAsync<Transaction>("Transactions", async query =>
        {
            var dbQuery = query.Where(t => t.AccountId == request.AccountId);

            if (request.Before.HasValue)
            {
                dbQuery = dbQuery.Where(t =>
                    t.Date < request.Before.Value || (t.Date == request.Before.Value && t.Id < request.BeforeId)
                );
            }

            transactions = dbQuery.OrderByDescending(t => t.Date).Limit(request.PageSize).ToList();
            await Task.CompletedTask;
        });

        return transactions;
    }
}
