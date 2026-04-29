using MyMoney.Core.Database;
using MyMoney.Core.Exports;
using MyMoney.Core.Models;

namespace MyMoney.Tests.CoreTests;

[TestClass]
public class TransactionCsvExporterTests
{
    private DatabaseManager _databaseManager = null!;
    private TransactionCsvExporter _exporter = null!;
    private string _tempFilePath = null!;

    [TestInitialize]
    public void Setup()
    {
        _databaseManager = new DatabaseManager(new MemoryStream());
        _exporter = new TransactionCsvExporter(_databaseManager);
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"mymoney-export-{Guid.NewGuid():N}.csv");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _databaseManager.Dispose();
        if (File.Exists(_tempFilePath))
            File.Delete(_tempFilePath);
    }

    [TestMethod]
    public async Task ExportAsync_SelectedAccountOnly_ExportsOnlyThatAccount()
    {
        _databaseManager.WriteCollection("Accounts", [
            new Account { Id = 1, AccountName = "Checking", Total = new Currency(1000m) },
            new Account { Id = 2, AccountName = "Savings", Total = new Currency(2000m) },
        ]);

        _databaseManager.WriteCollection("Transactions", [
            new Transaction(new DateTime(2026, 4, 10), "Store A", new Category { Name = "Food" }, new Currency(-10m), "")
            {
                AccountId = 1,
                Reconciled = true,
            },
            new Transaction(new DateTime(2026, 4, 11), "Store B", new Category { Name = "Travel" }, new Currency(-20m), "")
            {
                AccountId = 2,
                Reconciled = false,
            },
        ]);

        var result = await _exporter.ExportAsync(new TransactionCsvExportOptions
        {
            ExportAllAccounts = false,
            AccountId = 1,
            UseDateRange = false,
            SelectedFields = [TransactionExportField.AccountName, TransactionExportField.Payee, TransactionExportField.Amount],
            OutputFilePath = _tempFilePath,
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.RowCount);

        var lines = File.ReadAllLines(_tempFilePath);
        Assert.AreEqual("Account Name,Payee,Amount", lines[0]);
        Assert.IsTrue(lines[1].Contains("Checking"));
        Assert.IsTrue(lines[1].Contains("Store A"));
        Assert.IsFalse(lines[1].Contains("Store B"));
    }

    [TestMethod]
    public async Task ExportAsync_DateRangeInclusive_UsesSelectedColumnsInOrder()
    {
        _databaseManager.WriteCollection("Accounts", [
            new Account { Id = 1, AccountName = "Checking", Total = new Currency(1000m) },
        ]);

        _databaseManager.WriteCollection("Transactions", [
            new Transaction(new DateTime(2026, 4, 1), "Start", new Category { Name = "Cat1" }, new Currency(5m), "")
            {
                AccountId = 1,
                Reconciled = true,
            },
            new Transaction(new DateTime(2026, 4, 30), "End", new Category { Name = "Cat2" }, new Currency(15m), "")
            {
                AccountId = 1,
                Reconciled = false,
            },
            new Transaction(new DateTime(2026, 5, 1), "Outside", new Category { Name = "Cat3" }, new Currency(25m), "")
            {
                AccountId = 1,
                Reconciled = false,
            },
        ]);

        var result = await _exporter.ExportAsync(new TransactionCsvExportOptions
        {
            ExportAllAccounts = true,
            UseDateRange = true,
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2026, 4, 30),
            SelectedFields = [TransactionExportField.Date, TransactionExportField.Reconciled, TransactionExportField.Payee],
            OutputFilePath = _tempFilePath,
        });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.RowCount);

        var lines = File.ReadAllLines(_tempFilePath);
        Assert.AreEqual("Date,Reconciled,Payee", lines[0]);
        Assert.AreEqual("2026-04-01,True,Start", lines[1]);
        Assert.AreEqual("2026-04-30,False,End", lines[2]);
    }
}
