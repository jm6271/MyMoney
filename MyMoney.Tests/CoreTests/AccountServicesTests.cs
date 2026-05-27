using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Services.Accounts;

namespace MyMoney.Tests.CoreTests;

[TestClass]
public class AccountServicesTests
{
    [TestMethod]
    public void ValidateTransactionAmount_WhenDebitExceedsBalance_ReturnsFailure()
    {
        var service = new AccountValidationService();
        var account = new Account { Total = new Currency(50m) };

        var result = service.ValidateTransactionAmount(new Currency(75m), account);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(AccountValidationService.InsufficientFunds, result.ErrorCode);
    }

    [TestMethod]
    public void ApplyTransactionChange_ForAddEditDelete_MatchesExistingBalanceMath()
    {
        var service = new AccountTransactionService(new DatabaseManager(new MemoryStream()));
        var account = new Account { Total = new Currency(100m) };
        var oldTransaction = new Transaction(DateTime.Today, "Old", new(), new Currency(-20m), "");
        var newTransaction = new Transaction(DateTime.Today, "New", new(), new Currency(-10m), "");

        service.ApplyTransactionChange(account, null, oldTransaction);
        Assert.AreEqual(80m, account.Total.Value);

        service.ApplyTransactionChange(account, oldTransaction, newTransaction);
        Assert.AreEqual(90m, account.Total.Value);

        service.ApplyTransactionChange(account, newTransaction);
        Assert.AreEqual(100m, account.Total.Value);
    }

    [TestMethod]
    public void Transfer_CreatesTransactionsAndUpdatesAccountTotals()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        var service = new AccountTransactionService(databaseManager);
        var from = new Account { Id = 1, AccountName = "Checking", Total = new Currency(100m) };
        var to = new Account { Id = 2, AccountName = "Savings", Total = new Currency(20m) };

        var (fromTransaction, toTransaction) = service.Transfer(from, to, new Currency(30m));

        Assert.AreEqual(70m, from.Total.Value);
        Assert.AreEqual(50m, to.Total.Value);
        Assert.AreEqual(1, fromTransaction.AccountId);
        Assert.AreEqual(2, toTransaction.AccountId);
        Assert.AreEqual("Transfer to Savings", fromTransaction.Payee);
        Assert.AreEqual("Transfer from Checking", toTransaction.Payee);
        Assert.AreEqual(-30m, fromTransaction.Amount.Value);
        Assert.AreEqual(30m, toTransaction.Amount.Value);
        Assert.AreEqual("Transfer", fromTransaction.Memo);
        Assert.AreEqual("Transfer", toTransaction.Memo);
    }

    [TestMethod]
    public void CreateBalanceUpdateTransaction_UsesDifferenceBetweenOldAndNewBalance()
    {
        var service = new AccountTransactionService(new DatabaseManager(new MemoryStream()));
        var account = new Account { Id = 7, Total = new Currency(100m) };
        var date = new DateTime(2026, 5, 1);

        var transaction = service.CreateBalanceUpdateTransaction(account, new Currency(125m), date);

        Assert.AreEqual(7, transaction.AccountId);
        Assert.AreEqual(25m, transaction.Amount.Value);
        Assert.AreEqual(date, transaction.Date);
        Assert.AreEqual("Balance update", transaction.Payee);
        Assert.AreEqual("Balance update", transaction.Memo);
    }

    [TestMethod]
    public async Task GetTransactionsPage_OrdersByDateDescendingAndUsesBoundary()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        databaseManager.WriteCollection(
            "Transactions",
            new List<Transaction>
            {
                new(new DateTime(2026, 5, 4), "A", new(), new Currency(1m), "") { Id = 1, AccountId = 1 },
                new(new DateTime(2026, 5, 3), "B", new(), new Currency(1m), "") { Id = 2, AccountId = 1 },
                new(new DateTime(2026, 5, 2), "C", new(), new Currency(1m), "") { Id = 3, AccountId = 1 },
                new(new DateTime(2026, 5, 1), "D", new(), new Currency(1m), "") { Id = 4, AccountId = 2 },
            }
        );
        var service = new TransactionQueryService(databaseManager);

        var firstPage = await service.GetTransactionsPage(new TransactionPageRequest(1, null, 0, 2));
        var secondPage = await service.GetTransactionsPage(
            new TransactionPageRequest(1, firstPage[^1].Date, firstPage[^1].Id, 2)
        );

        CollectionAssert.AreEqual(new[] { "A", "B" }, firstPage.Select(t => t.Payee).ToArray());
        CollectionAssert.AreEqual(new[] { "C" }, secondPage.Select(t => t.Payee).ToArray());
    }
}
