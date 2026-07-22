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

                if (!savingsCategory.TransactionIds.Contains(transaction.Id))
                    savingsCategory.TransactionIds.Add(transaction.Id);
                savingsCategory.CurrentBalance += transaction.Amount;
                changed = true;
                break;

            case TransactionChangeOperation.Edit when oldTransaction != null:
                var oldIsSavings = oldTransaction.Category.Group == "Savings";
                var newIsSavings = transaction.Category.Group == "Savings";
                var sameSavingsCategory =
                    oldIsSavings
                    && newIsSavings
                    && transaction.Category.Name == oldTransaction.Category.Name;

                if (sameSavingsCategory)
                {
                    var categoryToUpdate = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                        x.CategoryName == transaction.Category.Name
                    );

                    if (categoryToUpdate == null)
                        break;

                    if (!categoryToUpdate.TransactionIds.Contains(oldTransaction.Id))
                        break;

                    categoryToUpdate.CurrentBalance -= oldTransaction.Amount;
                    categoryToUpdate.CurrentBalance += transaction.Amount;

                    changed = true;
                }
                else
                {
                    if (oldIsSavings)
                    {
                        var originalCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                            x.CategoryName == oldTransaction.Category.Name
                        );
                        if (originalCategory?.TransactionIds.Remove(oldTransaction.Id) == true)
                        {
                            originalCategory.CurrentBalance -= oldTransaction.Amount;
                            changed = true;
                        }
                    }

                    if (newIsSavings)
                    {
                        var newCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                            x.CategoryName == transaction.Category.Name
                        );
                        if (newCategory != null && !newCategory.TransactionIds.Contains(transaction.Id))
                        {
                            newCategory.TransactionIds.Add(transaction.Id);
                            newCategory.CurrentBalance += transaction.Amount;
                            changed = true;
                        }
                    }
                }
                break;

            case TransactionChangeOperation.Delete:
                var containingCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                    x.CategoryName == transaction.Category.Name
                );

                if (containingCategory == null)
                    break;

                if (containingCategory.TransactionIds.Contains(transaction.Id))
                {
                    containingCategory.CurrentBalance -= transaction.Amount;
                    containingCategory.TransactionIds.Remove(transaction.Id);
                    changed = true;
                }
                break;
        }

        if (changed)
            budgetCollection.SaveBudgetCollection();

        return new(changed, changed ? null : "NoMatchingCategoryOrTransaction");
    }
}
