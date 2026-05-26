using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed class BudgetComputationService
{
    public BudgetTotals CalculateTotals(Budget budget)
    {
        var incomeTotal = new Currency(budget.BudgetIncomeItems.Sum(item => item.Amount.Value));
        var expenseSum = budget.BudgetExpenseItems.Sum(item => item.CategoryTotal.Value);
        var savingsSum = budget.BudgetSavingsCategories.Sum(item => item.BudgetedAmount.Value);
        var expenseTotal = new Currency(expenseSum + savingsSum);

        return new BudgetTotals(incomeTotal, expenseTotal, incomeTotal - expenseTotal);
    }
}
