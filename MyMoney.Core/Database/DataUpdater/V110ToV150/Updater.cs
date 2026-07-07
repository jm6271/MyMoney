using MyMoney.Core.Database;
using MyMoney.Core.Models;
using OldBudget = MyMoney.DataUpdater.V110ToV150.OldModels.Budget;
using OldTransaction = MyMoney.DataUpdater.V110ToV150.OldModels.Transaction;

namespace MyMoney.DataUpdater.V110ToV150;

internal static class Updater
{
    public static void Update(IDatabaseManager databaseManager)
    {
        var oldTransactions = databaseManager.GetCollection<OldTransaction>("Transactions");
        var transactions = oldTransactions.Select(ConvertTransaction).ToList();
        var transactionsByHash = oldTransactions
            .Where(x => !string.IsNullOrEmpty(x.TransactionHash))
            .GroupBy(x => x.TransactionHash)
            .ToDictionary(x => x.Key, x => transactions[oldTransactions.IndexOf(x.First())]);

        var budgets = new List<Budget>();
        foreach (var oldBudget in databaseManager.GetCollection<OldBudget>("Budgets"))
        {
            var budget = new Budget
            {
                Id = oldBudget.Id,
                BudgetTitle = oldBudget.BudgetTitle,
                BudgetDate = oldBudget.BudgetDate,
                BudgetIncomeItems = oldBudget.BudgetIncomeItems,
                BudgetExpenseItems = oldBudget.BudgetExpenseItems,
            };

            foreach (var oldCategory in oldBudget.BudgetSavingsCategories)
            {
                var category = new Core.Models.BudgetSavingsCategory
                {
                    Id = oldCategory.Id,
                    CategoryName = oldCategory.CategoryName,
                    CurrentBalance = oldCategory.CurrentBalance,
                    BudgetedAmount = oldCategory.BudgetedAmount,
                    Spent = oldCategory.Spent,
                };
                if (!string.IsNullOrEmpty(oldCategory.CategoryHash))
                    category.CategoryHash = oldCategory.CategoryHash;

                foreach (var oldEmbeddedTransaction in oldCategory.Transactions)
                {
                    if (!transactionsByHash.TryGetValue(oldEmbeddedTransaction.TransactionHash, out var transaction))
                    {
                        transaction = ConvertTransaction(oldEmbeddedTransaction);
                        transaction.Id = 0;
                        transaction.AccountId = -1;
                        databaseManager.Insert("Transactions", transaction);
                        transactions.Add(transaction);
                        if (!string.IsNullOrEmpty(oldEmbeddedTransaction.TransactionHash))
                            transactionsByHash[oldEmbeddedTransaction.TransactionHash] = transaction;
                    }

                    if (!category.TransactionIds.Contains(transaction.Id))
                        category.TransactionIds.Add(transaction.Id);
                }

                category.PlannedTransactionId = ResolveId(
                    oldCategory.PlannedTransactionHash,
                    transactionsByHash
                );
                category.BalanceTransactionId = ResolveId(
                    oldCategory.BalanceTransactionHash,
                    transactionsByHash
                );
                budget.BudgetSavingsCategories.Add(category);
            }

            budgets.Add(budget);
        }

        databaseManager.WriteCollection("Transactions", transactions);
        databaseManager.WriteCollection("Budgets", budgets);

        var settings = databaseManager.GetSettingsDictionary("ApplicationSettings");
        settings["DataVersion"] = "1.5.0";
        databaseManager.WriteSettingsDictionary("ApplicationSettings", settings);
    }

    private static Transaction ConvertTransaction(OldTransaction oldTransaction)
    {
        return new Transaction(
            oldTransaction.Date,
            oldTransaction.Payee,
            oldTransaction.Category,
            oldTransaction.Amount,
            oldTransaction.Memo
        )
        {
            Id = oldTransaction.Id,
            TransactionDetail = oldTransaction.TransactionDetail,
            Reconciled = oldTransaction.Reconciled,
            AccountId = oldTransaction.AccountId,
        };
    }

    private static int ResolveId(
        string hash,
        IReadOnlyDictionary<string, Transaction> transactionsByHash
    )
    {
        return !string.IsNullOrEmpty(hash) && transactionsByHash.TryGetValue(hash, out var transaction)
            ? transaction.Id
            : 0;
    }

}
