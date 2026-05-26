using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed class SavingsCategoryTransactionSyncService : ISavingsCategoryTransactionSyncService
{
    private readonly IDatabaseManager _databaseManager;

    public SavingsCategoryTransactionSyncService(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public SavingsCategorySyncResult ApplyTransactionChangeToCurrentBudget(
        Transaction transaction,
        TransactionChangeOperation operation,
        Transaction? oldTransaction = null
    )
    {
        if (transaction.Category.Group != "Savings" && operation is TransactionChangeOperation.Add or TransactionChangeOperation.Delete)
            return new(false, "NotSavings");

        BudgetCollection budgetCollection = new(_databaseManager);
        if (!budgetCollection.DoesCurrentBudgetExist())
            return new(false, "NoCurrentBudget");

        var currentBudget = budgetCollection.Budgets[budgetCollection.GetCurrentBudgetIndex()];
        var changed = false;

        switch (operation)
        {
            case TransactionChangeOperation.Add:
                var savingsCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                    x.CategoryName == transaction.Category.Name
                );

                if (savingsCategory == null)
                    break;

                savingsCategory.Transactions.Add(transaction);
                savingsCategory.CurrentBalance += transaction.Amount;
                changed = true;
                break;

            case TransactionChangeOperation.Edit when oldTransaction != null:
                if (transaction.Category.Name == oldTransaction.Category.Name)
                {
                    if (transaction.Category.Group != "Savings")
                        break;

                    var categoryToUpdate = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                        x.CategoryName == transaction.Category.Name
                    );

                    if (categoryToUpdate == null)
                        break;

                    var transactionToUpdate = categoryToUpdate.Transactions.FirstOrDefault(x =>
                        x.TransactionHash == oldTransaction.TransactionHash
                    );
                    if (transactionToUpdate == null)
                        break;

                    categoryToUpdate.CurrentBalance -= oldTransaction.Amount;
                    categoryToUpdate.CurrentBalance += transaction.Amount;

                    var index = categoryToUpdate.Transactions.IndexOf(transactionToUpdate);
                    categoryToUpdate.Transactions[index] = transaction;
                    changed = true;
                }
                else
                {
                    var originalCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                        x.CategoryName == oldTransaction.Category.Name
                    );
                    if (originalCategory == null)
                        break;

                    var transactionInOldCategory = originalCategory.Transactions.FirstOrDefault(x =>
                        x.TransactionHash == oldTransaction.TransactionHash
                    );
                    if (transactionInOldCategory == null)
                        break;

                    originalCategory.Transactions.Remove(transactionInOldCategory);
                    originalCategory.CurrentBalance -= oldTransaction.Amount;
                    changed = true;

                    if (transaction.Category.Group == "Savings")
                    {
                        var newCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                            x.CategoryName == transaction.Category.Name
                        );

                        if (newCategory == null)
                            break;

                        newCategory.Transactions.Add(transaction);
                        newCategory.CurrentBalance += transaction.Amount;
                    }
                }
                break;

            case TransactionChangeOperation.Delete:
                var containingCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                    x.CategoryName == transaction.Category.Name
                );

                if (containingCategory == null)
                    break;

                var transactionToDelete = containingCategory.Transactions.FirstOrDefault(x =>
                    x.TransactionHash == transaction.TransactionHash
                );
                if (transactionToDelete != null)
                {
                    containingCategory.CurrentBalance -= transaction.Amount;
                    containingCategory.Transactions.Remove(transactionToDelete);
                    changed = true;
                }
                break;
        }

        if (changed)
            budgetCollection.SaveBudgetCollection();

        return new(changed, changed ? null : "NoMatchingCategoryOrTransaction");
    }
}
