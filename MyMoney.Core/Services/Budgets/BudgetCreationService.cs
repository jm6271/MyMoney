using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed class BudgetCreationService : IBudgetCreationService
{
    private readonly IDatabaseManager _databaseManager;

    public BudgetCreationService(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public async Task<BudgetCreationResult> CreateBudget(DateTime selectedMonth)
    {
        Budget newBudget = new()
        {
            BudgetTitle = selectedMonth.ToString("MMMM, yyyy"),
            BudgetDate = selectedMonth,
        };

        var mostRecentDate = await GetMostRecentBudgetDateBefore(selectedMonth);
        if (mostRecentDate == null)
            return new(newBudget, null);

        var sourceBudget = await GetBudgetForMonth(mostRecentDate.Value);
        if (sourceBudget == null)
            return new(newBudget, mostRecentDate);

        foreach (var item in sourceBudget.BudgetIncomeItems)
            newBudget.BudgetIncomeItems.Add((BudgetItem)item.Clone());

        foreach (var item in sourceBudget.BudgetSavingsCategories)
        {
            BudgetSavingsCategory newSavingsCategory = (BudgetSavingsCategory)item.Clone();

            Transaction balanceCarriedForward = new(
                newBudget.BudgetDate.AddDays(-1),
                "",
                new Category() { Group = "Savings", Name = item.CategoryName },
                item.CurrentBalance,
                "Balance carried forward"
            )
            {
                TransactionDetail = sourceBudget.BudgetDate.ToString("MMM") + " balance",
            };
            newSavingsCategory.Transactions.Add(balanceCarriedForward);

            Transaction appliedBudgetedAmount = new(
                newBudget.BudgetDate,
                "",
                new Category() { Group = "Savings", Name = item.CategoryName },
                item.BudgetedAmount,
                "Planned This Month"
            )
            {
                TransactionDetail = "Planned This Month",
            };

            newSavingsCategory.Transactions.Add(appliedBudgetedAmount);
            newSavingsCategory.PlannedTransactionHash = appliedBudgetedAmount.TransactionHash;
            newSavingsCategory.BalanceTransactionHash = balanceCarriedForward.TransactionHash;

            newSavingsCategory.CurrentBalance += newSavingsCategory.BudgetedAmount;

            newBudget.BudgetSavingsCategories.Add(newSavingsCategory);
        }

        foreach (var item in sourceBudget.BudgetExpenseItems)
            newBudget.BudgetExpenseItems.Add((BudgetExpenseCategory)item.Clone());

        return new(newBudget, mostRecentDate);
    }

    public async Task<DateTime?> GetMostRecentBudgetDateBefore(DateTime selectedMonth)
    {
        DateTime? mostRecentDate = null;
        await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
        {
            mostRecentDate = query
                .Where(p => p.BudgetDate < selectedMonth)
                .OrderByDescending(p => p.BudgetDate)
                .FirstOrDefault()
                ?.BudgetDate;
            await Task.CompletedTask;
        });

        return mostRecentDate;
    }

    private async Task<Budget?> GetBudgetForMonth(DateTime month)
    {
        Budget? budget = null;
        await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
        {
            budget = query
                .Where(p => p.BudgetDate.Month == month.Month && p.BudgetDate.Year == month.Year)
                .FirstOrDefault();
            await Task.CompletedTask;
        });

        return budget;
    }
}
