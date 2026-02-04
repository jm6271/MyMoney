using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;

namespace MyMoney.Tests.ReportsTests;

[TestClass]
public class NetWorthCalculatorTests
{
    private DatabaseManager _databaseManager = null!;
    private NetWorthCalculator _calculator = null!;

    [TestInitialize]
    public void Setup()
    {
        _databaseManager = new(new MemoryStream());
        _calculator = new NetWorthCalculator(_databaseManager);
    }

    [TestMethod]
    public async Task GetNetWorthSinceStartDate_ThrowsException_WhenStartDateInFuture()
    {
        // Arrange
        var futureDate = DateTime.Today.AddDays(1);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => _calculator.GetNetWorthSinceStartDate(futureDate));
    }

    [TestMethod]
    public async Task GetNetWorthSinceStartDate_ReturnsCorrectNetWorthData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-4);
        
        _databaseManager.WriteCollection("Accounts", [
            new Account
            {
                Total = new Currency(1000m),
                AccountName = "Account 1",
            },
            new Account
            {
                Total = new Currency(500m),
                AccountName = "Account 2",
            },
        ]);

        _databaseManager.WriteCollection("Transactions", [
            new Transaction(DateTime.Today.AddDays(-1), "Payee1", new Category(), new Currency(200m), "Memo1"),
            new Transaction(DateTime.Today, "Payee2", new Category(), new Currency(300m), "Memo2"),
            new Transaction(DateTime.Today.AddDays(-3), "Payee3", new Category(), new Currency(100m), "Memo3"),
        ]);

        // Act
        var result = await _calculator.GetNetWorthSinceStartDate(startDate);

        // Assert
        Assert.HasCount(5, result);
        Assert.AreEqual(900m, result[startDate]);
        Assert.AreEqual(1000m, result[startDate.AddDays(1)]);
        Assert.AreEqual(1000m, result[startDate.AddDays(2)]);
        Assert.AreEqual(1200m, result[startDate.AddDays(3)]);
        Assert.AreEqual(1500m, result[startDate.AddDays(4)]);
    }
}
