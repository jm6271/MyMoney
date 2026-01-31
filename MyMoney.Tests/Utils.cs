using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Tests
{
    internal class Utils
    {
        public static void SetupTransactionsMock(Mock<IDatabaseManager> mockDatabaseService, List<Transaction> transactions)
        {
            var mockCollection = new Mock<ILiteCollection<Transaction>>();
            var mockQueryable = new Mock<ILiteQueryable<Transaction>>();

            mockCollection.Setup(c => c.Query()).Returns(mockQueryable.Object);
            mockQueryable
                .Setup(q => q.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<Transaction, bool>> expr) =>
                {
                    var filtered = transactions.Where(expr.Compile()).ToList();
                    var chainedMock = new Mock<ILiteQueryable<Transaction>>();
                    chainedMock
                        .Setup(q => q.OrderByDescending(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, object>>>()))
                        .Returns((System.Linq.Expressions.Expression<Func<Transaction, object>> orderExpr) =>
                        {
                            var ordered = filtered.OrderByDescending(orderExpr.Compile()).ToList();
                            var finalMock = new Mock<ILiteQueryable<Transaction>>();
                            finalMock.Setup(q => q.ToList()).Returns(ordered);
                            return finalMock.Object;
                        });
                    return chainedMock.Object;
                });

            var mockDb = new Mock<LiteDatabase>();
            mockDb.Setup(m => m.GetCollection<Transaction>("Transactions")).Returns(mockCollection.Object);

            mockDatabaseService
                .Setup(db => db.ExecuteAsync(It.IsAny<Func<LiteDatabase, Task>>()))
                .Callback((Func<LiteDatabase, Task> action) => action(mockDb.Object).Wait())
                .Returns(Task.CompletedTask);
        }
    }
}
