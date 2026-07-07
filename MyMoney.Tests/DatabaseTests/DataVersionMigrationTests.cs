using System.Collections.ObjectModel;
using LiteDB;
using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Tests.DatabaseTests;

[TestClass]
public class DataVersionMigrationTests
{
    [TestMethod]
    public void EnsureDataVersion_MigratesSavingsTransactionsFromV110ToV150()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        databaseManager.WriteSettingsDictionary(
            "ApplicationSettings",
            new Dictionary<string, string> { ["DataVersion"] = "1.1.0" }
        );

        var accountTransaction = CreateLegacyTransaction("account-hash", new Currency(-25m), 1);
        databaseManager.Insert("Transactions", accountTransaction);

        var plannedTransaction = CreateLegacyTransaction("planned-hash", new Currency(50m), -1);
        plannedTransaction.TransactionDetail = "Planned This Month";
        var category = new LegacySavingsCategory
        {
            CategoryName = "Emergency",
            CurrentBalance = new Currency(125m),
            BudgetedAmount = new Currency(50m),
            PlannedTransactionHash = plannedTransaction.TransactionHash,
            CategoryHash = "category-hash",
            Transactions = [accountTransaction, plannedTransaction],
        };
        databaseManager.Insert(
            "Budgets",
            new LegacyBudget
            {
                BudgetTitle = "July, 2026",
                BudgetDate = new DateTime(2026, 7, 1),
                BudgetSavingsCategories = [category],
            }
        );

        var result = new DataVersionManager(databaseManager).EnsureDataVersion();

        Assert.IsTrue(result);
        Assert.AreEqual(
            "1.5.0",
            databaseManager.GetSettingsDictionary("ApplicationSettings")["DataVersion"]
        );

        var transactions = databaseManager.GetCollection<Transaction>("Transactions");
        var budget = databaseManager.GetCollection<Budget>("Budgets").Single();
        var migratedCategory = budget.BudgetSavingsCategories.Single();
        var migratedAccountTransaction = transactions.Single(x => x.AccountId == 1);
        var migratedPlannedTransaction = transactions.Single(x => x.AccountId == -1);

        Assert.HasCount(2, transactions);
        Assert.HasCount(2, migratedCategory.TransactionIds);
        Assert.Contains(migratedAccountTransaction.Id, migratedCategory.TransactionIds);
        Assert.Contains(migratedPlannedTransaction.Id, migratedCategory.TransactionIds);
        Assert.AreEqual(migratedPlannedTransaction.Id, migratedCategory.PlannedTransactionId);
        Assert.AreEqual("category-hash", migratedCategory.CategoryHash);

        var transactionDocument = databaseManager
            .GetCollection<BsonDocument>("Transactions")
            .First(x => x["_id"].AsInt32 == migratedAccountTransaction.Id);
        var savingsDocument = databaseManager
            .GetCollection<BsonDocument>("Budgets")
            .Single()["BudgetSavingsCategories"]
            .AsArray.Single()
            .AsDocument;
        Assert.IsFalse(transactionDocument.ContainsKey("TransactionHash"));
        Assert.IsFalse(savingsDocument.ContainsKey("Transactions"));
        Assert.IsFalse(savingsDocument.ContainsKey("PlannedTransactionHash"));
    }

    [TestMethod]
    public void EnsureDataVersion_ChainsV100AccountMigrationIntoV150()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        databaseManager.WriteSettingsDictionary(
            "ApplicationSettings",
            new Dictionary<string, string> { ["DataVersion"] = "1.0.0" }
        );

        var transaction = CreateLegacyTransaction("shared-hash", new Currency(-25m), -1);
        databaseManager.Insert(
            "Accounts",
            new LegacyAccount
            {
                Id = 7,
                AccountName = "Checking",
                Total = new Currency(100m),
                Transactions = [transaction],
            }
        );
        databaseManager.Insert(
            "Budgets",
            new LegacyBudget
            {
                BudgetTitle = "July, 2026",
                BudgetDate = new DateTime(2026, 7, 1),
                BudgetSavingsCategories =
                [
                    new LegacySavingsCategory
                    {
                        CategoryName = "Emergency",
                        Transactions = [transaction],
                    },
                ],
            }
        );

        var result = new DataVersionManager(databaseManager).EnsureDataVersion();

        Assert.IsTrue(result);
        var migratedTransaction = databaseManager.GetCollection<Transaction>("Transactions").Single();
        var migratedCategory = databaseManager
            .GetCollection<Budget>("Budgets")
            .Single().BudgetSavingsCategories.Single();
        Assert.AreEqual(7, migratedTransaction.AccountId);
        Assert.AreEqual(migratedTransaction.Id, migratedCategory.TransactionIds.Single());
        Assert.AreEqual(
            "1.5.0",
            databaseManager.GetSettingsDictionary("ApplicationSettings")["DataVersion"]
        );
    }

    private static LegacyTransaction CreateLegacyTransaction(string hash, Currency amount, int accountId)
    {
        return new LegacyTransaction
        {
            Date = new DateTime(2026, 7, 1),
            Category = new Category { Group = "Savings", Name = "Emergency" },
            Amount = amount,
            TransactionHash = hash,
            AccountId = accountId,
        };
    }

    private sealed class LegacyTransaction
    {
        public int Id { get; set; }
        public string TransactionHash { get; set; } = "";
        public string Payee { get; set; } = "";
        public Category Category { get; set; } = new();
        public Currency Amount { get; set; } = new(0m);
        public string Memo { get; set; } = "";
        public string TransactionDetail { get; set; } = "";
        public DateTime Date { get; set; }
        public bool Reconciled { get; set; }
        public int AccountId { get; set; } = -1;
    }

    private sealed class LegacyBudget
    {
        public int Id { get; set; }
        public string BudgetTitle { get; set; } = "";
        public DateTime BudgetDate { get; set; }
        public ObservableCollection<BudgetItem> BudgetIncomeItems { get; set; } = [];
        public ObservableCollection<BudgetExpenseCategory> BudgetExpenseItems { get; set; } = [];
        public ObservableCollection<LegacySavingsCategory> BudgetSavingsCategories { get; set; } = [];
    }

    private sealed class LegacyAccount
    {
        public int Id { get; set; }
        public ObservableCollection<LegacyTransaction> Transactions { get; set; } = [];
        public string AccountName { get; set; } = "";
        public Currency Total { get; set; } = new(0m);
    }

    private sealed class LegacySavingsCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = "";
        public Currency CurrentBalance { get; set; } = new(0m);
        public Currency BudgetedAmount { get; set; } = new(0m);
        public Currency Spent { get; set; } = new(0m);
        public string PlannedTransactionHash { get; set; } = "";
        public string BalanceTransactionHash { get; set; } = "";
        public ObservableCollection<LegacyTransaction> Transactions { get; set; } = [];
        public string CategoryHash { get; set; } = "";
    }
}
