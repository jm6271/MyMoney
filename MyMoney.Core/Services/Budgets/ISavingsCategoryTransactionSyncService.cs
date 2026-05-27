using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public interface ISavingsCategoryTransactionSyncService
{
    SavingsCategorySyncResult ApplyTransactionChangeToCurrentBudget(
        Transaction transaction,
        TransactionChangeOperation operation,
        Transaction? oldTransaction = null
    );
}
