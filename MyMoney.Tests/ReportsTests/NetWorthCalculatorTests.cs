using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;

namespace MyMoney.Tests.ReportsTests;

[TestClass]
public class NetWorthCalculatorTests
{
    private Mock<IDatabaseManager> _mockDbManager = null!;
    private NetWorthCalculator _calculator = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockDbManager = new Mock<IDatabaseManager>();
        _calculator = new NetWorthCalculator(_mockDbManager.Object);
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
        var accounts = new List<Account>
        {
            new()
            {
                Total = new Currency(1000m),
                AccountName = "Account 1",
            },
            new()
            {
                Total = new Currency(500m),
                AccountName = "Account 2",
            },
        };

        var transactions = new List<Transaction>
        {
            new(DateTime.Today.AddDays(-1), "Payee1", new Category(), new Currency(200m), "Memo1"),
            new(DateTime.Today, "Payee2", new Category(), new Currency(300m), "Memo2"),
            new(DateTime.Today.AddDays(-3), "Payee3", new Category(), new Currency(100m), "Memo3"),
        };

        _mockDbManager.Setup(db => db.GetCollection<Account>("Accounts")).Returns(accounts);

        // Create a mock ILiteCollection that returns a queryable that can be filtered and sorted
        var mockCollection = new Mock<ILiteCollection<Transaction>>();
        var mockQueryable = new Mock<ILiteQueryable<Transaction>>();

        // Set up the Query() method to return the mockQueryable
        mockCollection.Setup(c => c.Query()).Returns(mockQueryable.Object);

        // When Where is called, return mockQueryable (for chaining)
        mockQueryable
            .Setup(q => q.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>()))
            .Returns((System.Linq.Expressions.Expression<Func<Transaction, bool>> expr) =>
            {
                var filtered = transactions.Where(expr.Compile()).ToList();
                var chainedMock = new Mock<ILiteQueryable<Transaction>>();
                
                // When OrderBy is called, return the filtered and ordered results via ToList()
                chainedMock
                    .Setup(q => q.OrderBy(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, object>>>()))
                    .Returns((System.Linq.Expressions.Expression<Func<Transaction, object>> orderExpr) =>
                    {
                        var ordered = filtered.OrderBy(orderExpr.Compile()).ToList();
                        var finalMock = new Mock<ILiteQueryable<Transaction>>();
                        finalMock.Setup(q => q.ToList()).Returns(ordered);
                        return finalMock.Object;
                    });

                return chainedMock.Object;
            });

        var mockDb = new Mock<LiteDatabase>();
        mockDb.Setup(m => m.GetCollection<Transaction>("Transactions")).Returns(mockCollection.Object);

        _mockDbManager
            .Setup(db => db.ExecuteAsync(It.IsAny<Func<LiteDatabase, Task>>()))
            .Callback((Func<LiteDatabase, Task> action) => action(mockDb.Object).Wait())
            .Returns(Task.CompletedTask);

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
