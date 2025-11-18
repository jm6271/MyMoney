using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public void GetNetWorthSinceStartDate_ThrowsException_WhenStartDateInFuture()
    {
        // Arrange
        var futureDate = DateTime.Today.AddDays(1);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => _calculator.GetNetWorthSinceStartDate(futureDate));
    }

    [TestMethod]
    public void GetNetWorthSinceStartDate_ReturnsCorrectNetWorthData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-4);
        var accounts = new List<Account>
        {
            new()
            {
                Total = new Currency(1000m),
                Transactions = new ObservableCollection<Transaction>
                {
                    new(DateTime.Today.AddDays(-1), "Payee1", new Category(), new Currency(200m), "Memo1"),
                    new(DateTime.Today, "Payee2", new Category(), new Currency(300m), "Memo2"),
                },
                AccountName = "Account 1",
            },
            new()
            {
                Total = new Currency(500m),
                Transactions = new ObservableCollection<Transaction>
                {
                    new(DateTime.Today.AddDays(-3), "Payee3", new Category(), new Currency(100m), "Memo3"),
                },
                AccountName = "Account 2",
            },
        };

        _mockDbManager.Setup(db => db.GetCollection<Account>("Accounts")).Returns(accounts);

        // Act
        var result = _calculator.GetNetWorthSinceStartDate(startDate);

        // Assert
        Assert.HasCount(5, result);
        Assert.AreEqual(900m, result[startDate]);
        Assert.AreEqual(1000m, result[startDate.AddDays(1)]);
        Assert.AreEqual(1000m, result[startDate.AddDays(2)]);
        Assert.AreEqual(1200m, result[startDate.AddDays(3)]);
        Assert.AreEqual(1500m, result[startDate.AddDays(4)]);
    }
}
