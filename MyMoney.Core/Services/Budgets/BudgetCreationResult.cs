using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed record BudgetCreationResult(Budget Budget, DateTime? CopiedFromBudgetDate);
