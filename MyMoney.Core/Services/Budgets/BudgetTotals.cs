using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed record BudgetTotals(Currency IncomeTotal, Currency ExpenseTotal, Currency LeftToBudget);
