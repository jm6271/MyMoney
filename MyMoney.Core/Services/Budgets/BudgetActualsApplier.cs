using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed class BudgetActualsApplier
{
    public void Apply(
        Budget budget,
        List<BudgetReportItem> incomeItems,
        List<BudgetReportItem> expenseItems,
        List<SavingsCategoryReportItem> savingsItems
    )
    {
        ApplyIncomeActuals(budget, incomeItems);
        ApplySavingsActuals(budget, savingsItems);
        ApplyExpenseActuals(budget, expenseItems);
    }

    public void ApplyIncomeActuals(Budget budget, List<BudgetReportItem> incomeItems)
    {
        for (var i = 0; i < Math.Min(incomeItems.Count, budget.BudgetIncomeItems.Count); i++)
            budget.BudgetIncomeItems[i].Actual = incomeItems[i].Actual;
    }

    public void ApplySavingsActuals(Budget budget, List<SavingsCategoryReportItem> savingsItems)
    {
        for (var i = 0; i < Math.Min(savingsItems.Count, budget.BudgetSavingsCategories.Count); i++)
            budget.BudgetSavingsCategories[i].Spent = savingsItems[i].Spent;
    }

    public void ApplyExpenseActuals(Budget budget, List<BudgetReportItem> expenseItems)
    {
        foreach (var expenseGroup in budget.BudgetExpenseItems)
        {
            foreach (var subItem in expenseGroup.SubItems)
            {
                var matchingItem = expenseItems.FirstOrDefault(item =>
                    item.Category == subItem.Category && item.Group == expenseGroup.CategoryName
                );

                if (matchingItem != null)
                    subItem.Actual = matchingItem.Actual;
            }
        }
    }
}
