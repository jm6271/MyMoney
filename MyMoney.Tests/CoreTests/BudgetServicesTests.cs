using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Services.Budgets;

namespace MyMoney.Tests.CoreTests;

[TestClass]
public class BudgetServicesTests
{
    [TestMethod]
    public void CalculateTotals_IncludesExpensesAndSavingsInExpenseTotal()
    {
        var budget = CreateBudget(new DateTime(2026, 5, 1));
        budget.BudgetIncomeItems.Add(new BudgetItem { Category = "Paycheck", Amount = new Currency(1000m) });
        budget.BudgetExpenseItems.Add(
            new BudgetExpenseCategory
            {
                CategoryName = "Bills",
                SubItems = [new BudgetItem { Category = "Rent", Amount = new Currency(600m) }],
            }
        );
        budget.BudgetSavingsCategories.Add(
            new BudgetSavingsCategory { CategoryName = "Emergency", BudgetedAmount = new Currency(100m) }
        );

        var totals = new BudgetComputationService().CalculateTotals(budget);

        Assert.AreEqual(1000m, totals.IncomeTotal.Value);
        Assert.AreEqual(700m, totals.ExpenseTotal.Value);
        Assert.AreEqual(300m, totals.LeftToBudget.Value);
    }

    [TestMethod]
    public void ValidationService_PreservesDuplicateAndReservedGroupRules()
    {
        var budget = CreateBudget(new DateTime(2026, 5, 1));
        budget.BudgetIncomeItems.Add(new BudgetItem { Category = "Paycheck" });
        budget.BudgetSavingsCategories.Add(new BudgetSavingsCategory { CategoryName = "Emergency" });
        budget.BudgetExpenseItems.Add(
            new BudgetExpenseCategory
            {
                CategoryName = "Bills",
                SubItems = [new BudgetItem { Category = "Rent" }],
            }
        );
        var service = new BudgetValidationService();

        Assert.IsTrue(service.DoesIncomeItemExist(budget, "Paycheck"));
        Assert.IsTrue(service.DoesSavingsCategoryExist(budget, "Emergency"));
        Assert.IsTrue(service.DoesExpenseGroupExist(budget, "Savings"));
        Assert.IsTrue(service.DoesExpenseGroupExist(budget, "Income"));
        Assert.IsTrue(service.DoesExpenseGroupExist(budget, "Bills"));
        Assert.IsTrue(service.DoesExpenseItemExist(budget, "Rent"));
        Assert.IsFalse(service.DoesExpenseItemExist(budget, "Groceries"));
    }

    [TestMethod]
    public async Task CreateBudget_CopiesPriorBudgetAndPreservesSavingsCarryForwardHashes()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        var priorDate = new DateTime(2026, 4, 1);
        var targetDate = new DateTime(2026, 5, 1);
        var priorBudget = CreateBudget(priorDate);
        priorBudget.BudgetIncomeItems.Add(new BudgetItem { Category = "Paycheck", Amount = new Currency(1000m) });
        priorBudget.BudgetSavingsCategories.Add(
            new BudgetSavingsCategory
            {
                CategoryName = "Emergency",
                CurrentBalance = new Currency(250m),
                BudgetedAmount = new Currency(50m),
            }
        );
        priorBudget.BudgetExpenseItems.Add(
            new BudgetExpenseCategory
            {
                CategoryName = "Bills",
                SubItems = [new BudgetItem { Category = "Rent", Amount = new Currency(600m) }],
            }
        );
        databaseManager.Insert("Budgets", priorBudget);
        var service = new BudgetCreationService(databaseManager);

        var result = await service.CreateBudget(targetDate);

        Assert.AreEqual(priorDate, result.CopiedFromBudgetDate);
        Assert.AreEqual(targetDate, result.Budget.BudgetDate);
        Assert.AreEqual("May, 2026", result.Budget.BudgetTitle);
        Assert.AreEqual(1, result.Budget.BudgetIncomeItems.Count);
        Assert.AreEqual(1, result.Budget.BudgetExpenseItems.Count);
        Assert.AreEqual(1, result.Budget.BudgetSavingsCategories.Count);

        var savings = result.Budget.BudgetSavingsCategories[0];
        Assert.AreEqual(300m, savings.CurrentBalance.Value);
        Assert.AreEqual(2, savings.Transactions.Count);
        Assert.AreEqual(savings.BalanceTransactionHash, savings.Transactions[0].TransactionHash);
        Assert.AreEqual(savings.PlannedTransactionHash, savings.Transactions[1].TransactionHash);
    }

    [TestMethod]
    public void BudgetActualsApplier_MapsActualsLikeViewModel()
    {
        var budget = CreateBudget(new DateTime(2026, 5, 1));
        budget.BudgetIncomeItems.Add(new BudgetItem { Category = "Paycheck" });
        budget.BudgetSavingsCategories.Add(new BudgetSavingsCategory { CategoryName = "Emergency" });
        budget.BudgetExpenseItems.Add(
            new BudgetExpenseCategory
            {
                CategoryName = "Bills",
                SubItems = [new BudgetItem { Category = "Rent" }],
            }
        );

        new BudgetActualsApplier().Apply(
            budget,
            [new BudgetReportItem { Actual = new Currency(1000m) }],
            [new BudgetReportItem { Group = "Bills", Category = "Rent", Actual = new Currency(500m) }],
            [new SavingsCategoryReportItem { Spent = new Currency(25m) }]
        );

        Assert.AreEqual(1000m, budget.BudgetIncomeItems[0].Actual.Value);
        Assert.AreEqual(25m, budget.BudgetSavingsCategories[0].Spent.Value);
        Assert.AreEqual(500m, budget.BudgetExpenseItems[0].SubItems[0].Actual.Value);
    }

    [TestMethod]
    public async Task FutureBudgetAdjustmentService_UpdatesMatchingFutureSavingsCategories()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        var original = new BudgetSavingsCategory { CategoryName = "Emergency", CurrentBalance = new Currency(100m) };
        var modified = (BudgetSavingsCategory)original.Clone();
        modified.CurrentBalance = new Currency(125m);
        var balanceTransaction = new Transaction(
            new DateTime(2026, 5, 31),
            "",
            new Category { Group = "Savings", Name = "Emergency" },
            new Currency(100m),
            ""
        );

        var futureCategory = (BudgetSavingsCategory)original.Clone();
        futureCategory.CurrentBalance = new Currency(100m);
        futureCategory.BalanceTransactionHash = balanceTransaction.TransactionHash;
        futureCategory.Transactions.Add(balanceTransaction);

        var futureBudget = CreateBudget(new DateTime(2026, 6, 1));
        futureBudget.BudgetSavingsCategories.Add(futureCategory);
        databaseManager.Insert("Budgets", futureBudget);

        await new FutureBudgetAdjustmentService(databaseManager).UpdateFutureSavingsCategories(
            new DateTime(2026, 5, 1),
            original,
            modified
        );

        var savedBudget = databaseManager.GetCollection<Budget>("Budgets").Single();
        var savedCategory = savedBudget.BudgetSavingsCategories[0];
        Assert.AreEqual(125m, savedCategory.CurrentBalance.Value);
        Assert.AreEqual(125m, savedCategory.Transactions[0].Amount.Value);
    }

    [TestMethod]
    public void SavingsCategoryTransactionSyncService_AppliesAddEditAndDeleteToCurrentBudget()
    {
        using var databaseManager = new DatabaseManager(new MemoryStream());
        var budget = CreateBudget(DateTime.Today);
        var category = new BudgetSavingsCategory { CategoryName = "Emergency", CurrentBalance = new Currency(0m) };
        budget.BudgetSavingsCategories.Add(category);
        databaseManager.WriteCollection("Budgets", [budget]);
        var service = new SavingsCategoryTransactionSyncService(databaseManager);

        var addedTransaction = new Transaction(
            DateTime.Today,
            "Transfer",
            new Category { Group = "Savings", Name = "Emergency" },
            new Currency(10m),
            ""
        );
        var addResult = service.ApplyTransactionChangeToCurrentBudget(
            addedTransaction,
            TransactionChangeOperation.Add
        );

        var editedTransaction = new Transaction(
            DateTime.Today,
            "Transfer",
            new Category { Group = "Savings", Name = "Emergency" },
            new Currency(15m),
            ""
        )
        {
            TransactionHash = addedTransaction.TransactionHash,
        };
        var editResult = service.ApplyTransactionChangeToCurrentBudget(
            editedTransaction,
            TransactionChangeOperation.Edit,
            addedTransaction
        );

        var deleteResult = service.ApplyTransactionChangeToCurrentBudget(
            editedTransaction,
            TransactionChangeOperation.Delete
        );

        var savedCategory = databaseManager.GetCollection<Budget>("Budgets").Single().BudgetSavingsCategories.Single();
        Assert.IsTrue(addResult.Changed);
        Assert.IsTrue(editResult.Changed);
        Assert.IsTrue(deleteResult.Changed);
        Assert.AreEqual(0m, savedCategory.CurrentBalance.Value);
        Assert.AreEqual(0, savedCategory.Transactions.Count);
    }

    private static Budget CreateBudget(DateTime date)
    {
        return new Budget { BudgetDate = date, BudgetTitle = date.ToString("MMMM, yyyy") };
    }
}
