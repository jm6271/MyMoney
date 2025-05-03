using MyMoney.ViewModels.Pages.ReportPages;
using Moq;
using MyMoney.Core.Models;

namespace MyMoney.Tests.ViewModelTests
{
    [TestClass]
    public class BudgetReportsViewModelTest
    {
        [TestMethod]
        public void Test_CalculateBudgetReport()
        {
            var mockDatabaseService = new Mock<Core.Database.IDatabaseReader>();
            mockDatabaseService.Setup(service => service.GetCollection<Budget>("Budgets")).Returns(
            [
                new Budget {
                    BudgetDate = DateTime.Now,
                    BudgetTitle = DateTime.Now.ToString("MMMM, yyy"),
                    BudgetIncomeItems = [
                        new BudgetItem { Category = "Income 1", Amount = new Currency(1000m) },
                        new BudgetItem { Category = "Income 2", Amount = new Currency(500) }
                        ],
                    BudgetExpenseItems = [
                        new BudgetExpenseCategory { CategoryName = "Category 1", SubItems = [
                            new () { Category = "Expense 1", Amount = new(105) },
                            new () { Category = "Expense 2", Amount = new(750) }
                            ]},
                        new BudgetExpenseCategory { CategoryName = "Category 2", SubItems = [
                            new () { Category = "Expense 3", Amount = new(250) },
                            new () { Category = "Expense 4", Amount = new(350) }
                            ]},
                        ]
                }
            ]);

            mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts")).Returns(
                [
                    new Account {
                        AccountName = "Account",
                        Total = new Currency(1000m),
                        Transactions = [
                            new Transaction(DateTime.Now.AddMonths(-2), "Job", new() { Name = "Income 1", Group = "Income" }, new Currency(330), "Paycheck"), // old income transaction
                            new Transaction(DateTime.Now.AddMonths(-1), "Fast Food, Inc.", new() { Name = "Expense 1", Group = "Food" }, new Currency(-15), "Lunch"), // old expense transaction
                            new Transaction(DateTime.Now, "Side Job", new() { Name = "Income 2", Group = "Income" }, new Currency(100), "Side Job"), // current income transaction
                            new Transaction(DateTime.Now, "Job", new() { Name = "Income 1", Group = "Income" }, new Currency(330), "Paycheck"), // another current income transaction
                            new Transaction(DateTime.Now, "Fast Food, Inc.", new() { Name = "Expense 1", Group = "Food" }, new Currency(-15), "Lunch"), // current expense transaction
                            new Transaction(DateTime.Now, "Gas Station, Inc.", new() { Name = "Expense 4", Group = "Auto" }, new Currency(-70), "Fill up car"), // current expense transaction
                            new Transaction(DateTime.Now, "Store, Inc.", new() { Name = "Expense 1", Group = "Food" }, new Currency(-100), "Groceries"), // current expense transaction
                            ]
                    }
                ]);

            var viewModel = new BudgetReportsViewModel(mockDatabaseService.Object);
            viewModel.CalculateReport(DateTime.Now);

            Assert.AreEqual(3, viewModel.IncomeItems.Count);
            Assert.AreEqual(5, viewModel.ExpenseItems.Count);

            // Make sure income items are correct
            Assert.AreEqual(330m, viewModel.IncomeItems[0].Actual.Value);
            Assert.AreEqual(100m, viewModel.IncomeItems[1].Actual.Value);
            Assert.AreEqual(430m, viewModel.IncomeItems[2].Actual.Value);

            // Make sure expense items are correct
            Assert.AreEqual(115m, viewModel.ExpenseItems[0].Actual.Value);
            Assert.AreEqual(0m, viewModel.ExpenseItems[1].Actual.Value);
            Assert.AreEqual(0m, viewModel.ExpenseItems[2].Actual.Value);
            Assert.AreEqual(70m, viewModel.ExpenseItems[3].Actual.Value);
            Assert.AreEqual(185m, viewModel.ExpenseItems[4].Actual.Value);

            // Make sure total is correct
            Assert.AreEqual(245m, viewModel.ReportTotal.Value);
        }
    }
}
