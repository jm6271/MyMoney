namespace MyMoney.Core.Services.Budgets;

public interface IBudgetCreationService
{
    Task<BudgetCreationResult> CreateBudget(DateTime selectedMonth);
    Task<DateTime?> GetMostRecentBudgetDateBefore(DateTime selectedMonth);
}
