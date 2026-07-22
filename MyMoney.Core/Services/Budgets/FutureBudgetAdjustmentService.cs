using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed class FutureBudgetAdjustmentService
{
    private readonly IDatabaseManager _databaseManager;

    public FutureBudgetAdjustmentService(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public async Task UpdateFutureSavingsCategories(
        DateTime earliestDate,
        BudgetSavingsCategory originalSavingsCategory,
        BudgetSavingsCategory modifiedSavingsCategory
    )
    {
        await Task.Run(async () =>
        {
            List<Budget> budgets = [];

            await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
            {
                budgets.AddRange(query.Where(p => p.BudgetDate > earliestDate).ToList());
                await Task.CompletedTask;
            });

            for (var i = 0; i < budgets.Count; i++)
            {
                for (var j = 0; j < budgets[i].BudgetSavingsCategories.Count; j++)
                {
                    var currentSavingsCategory = budgets[i].BudgetSavingsCategories[j];

                    if (
                        currentSavingsCategory.CategoryHash == modifiedSavingsCategory.CategoryHash
                        && currentSavingsCategory.BalanceTransactionId != 0
                    )
                    {
                        var balanceDifference =
                            modifiedSavingsCategory.CurrentBalance - originalSavingsCategory.CurrentBalance;

                        var transaction = _databaseManager
                            .GetCollection<Transaction>("Transactions")
                            .FirstOrDefault(x => x.Id == currentSavingsCategory.BalanceTransactionId);
                        if (transaction != null)
                        {
                            transaction.Amount += balanceDifference;
                            _databaseManager.Update("Transactions", transaction);
                        }

                        currentSavingsCategory.CurrentBalance += balanceDifference;
                    }
                }

                _databaseManager.Update("Budgets", budgets[i]);
            }
        });
    }
}
