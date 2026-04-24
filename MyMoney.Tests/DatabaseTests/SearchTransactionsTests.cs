using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Tests.DatabaseTests;

[TestClass]
public class SearchTransactionsTests
{
    private DatabaseManager _db = null!;

    private static Transaction MakeTransaction(int accountId, string payee, string categoryName, int id = 0, string memo = "")
    {
        var t = new Transaction(DateTime.Today, payee, new Category { Name = categoryName }, new Currency(10m), memo)
        {
            AccountId = accountId,
            Id = id
        };
        return t;
    }

    [TestInitialize]
    public void Setup()
    {
        _db = new DatabaseManager(new System.IO.MemoryStream());
    }

    [TestCleanup]
    public void Cleanup() => _db.Dispose();

    [TestMethod]
    public async Task SearchTransactionsAsync_ReturnsOnlyTransactionsForGivenAccount()
    {
        // Arrange
        var t1 = MakeTransaction(accountId: 1, payee: "Grocery Store", categoryName: "Food", id: 1);
        var t2 = MakeTransaction(accountId: 2, payee: "Grocery Store", categoryName: "Food", id: 2);
        _db.Insert("Transactions", t1);
        _db.Insert("Transactions", t2);

        // Act
        var results = await _db.SearchTransactionsAsync(accountId: 1, query: "Grocery");

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual(1, results[0].AccountId);
    }

    [TestMethod]
    public async Task SearchTransactionsAsync_MatchesOnPayee_CaseInsensitive()
    {
        // Arrange
        var t1 = MakeTransaction(accountId: 1, payee: "Amazon", categoryName: "Shopping", id: 1);
        var t2 = MakeTransaction(accountId: 1, payee: "Netflix", categoryName: "Entertainment", id: 2);
        _db.Insert("Transactions", t1);
        _db.Insert("Transactions", t2);

        // Act — uppercase query
        var results = await _db.SearchTransactionsAsync(accountId: 1, query: "AMAZON");

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("Amazon", results[0].Payee);
    }

    [TestMethod]
    public async Task SearchTransactionsAsync_MatchesOnCategoryName_CaseInsensitive()
    {
        // Arrange
        var t1 = MakeTransaction(accountId: 1, payee: "Store A", categoryName: "Groceries", id: 1);
        var t2 = MakeTransaction(accountId: 1, payee: "Store B", categoryName: "Entertainment", id: 2);
        _db.Insert("Transactions", t1);
        _db.Insert("Transactions", t2);

        // Act — mixed-case query
        var results = await _db.SearchTransactionsAsync(accountId: 1, query: "GROCERIES");

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("Groceries", results[0].Category.Name);
    }

    [TestMethod]
    public async Task SearchTransactionsAsync_MatchesOnMemo_CaseInsensitive()
    {
        // Arrange
        var t1 = MakeTransaction(accountId: 1, payee: "Store A", categoryName: "Shopping", id: 1, memo: "Monthly invoice #42");
        var t2 = MakeTransaction(accountId: 1, payee: "Store B", categoryName: "Shopping", id: 2, memo: "Cash back");
        _db.Insert("Transactions", t1);
        _db.Insert("Transactions", t2);

        // Act — uppercase query on memo only
        var results = await _db.SearchTransactionsAsync(accountId: 1, query: "INVOICE");

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("Monthly invoice #42", results[0].Memo);
    }

    [TestMethod]
    public async Task SearchTransactionsAsync_ReturnsEmptyList_WhenNoTransactionsMatch()
    {
        // Arrange
        var t1 = MakeTransaction(accountId: 1, payee: "Amazon", categoryName: "Shopping", id: 1);
        _db.Insert("Transactions", t1);

        // Act
        var results = await _db.SearchTransactionsAsync(accountId: 1, query: "zzznomatch");

        // Assert
        Assert.IsEmpty(results);
    }

    [TestMethod]
    public async Task SearchTransactionsAsync_ReturnsEmptyList_WhenAccountHasNoTransactions()
    {
        // Arrange — no transactions inserted

        // Act
        var results = await _db.SearchTransactionsAsync(accountId: 99, query: "anything");

        // Assert
        Assert.IsEmpty(results);
    }

    [TestMethod]
    public async Task SearchTransactionsAsync_PartialMatch_ReturnsMatchingTransactions()
    {
        // Arrange
        var t1 = MakeTransaction(accountId: 1, payee: "Whole Foods Market", categoryName: "Groceries", id: 1);
        var t2 = MakeTransaction(accountId: 1, payee: "Target", categoryName: "Shopping", id: 2);
        _db.Insert("Transactions", t1);
        _db.Insert("Transactions", t2);

        // Act — partial payee match
        var results = await _db.SearchTransactionsAsync(accountId: 1, query: "foods");

        // Assert
        Assert.HasCount(1, results);
        Assert.AreEqual("Whole Foods Market", results[0].Payee);
    }
}
