using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Budgets;

public sealed class BudgetValidationService
{
    public bool DoesIncomeItemExist(Budget budget, string item)
    {
        return budget.BudgetIncomeItems.Any(incomeCategory => incomeCategory.Category == item);
    }

    public bool DoesSavingsCategoryExist(Budget budget, string category)
    {
        return budget.BudgetSavingsCategories.Any(savingsCategory => savingsCategory.CategoryName == category);
    }

    public bool DoesExpenseGroupExist(Budget budget, string groupName)
    {
        if (groupName == "Savings" || groupName == "Income")
            return true;

        return budget.BudgetExpenseItems.Any(expenseGroup => expenseGroup.CategoryName == groupName);
    }

    public bool DoesExpenseItemExist(Budget budget, string item)
    {
        return budget.BudgetExpenseItems.Any(expenseGroup =>
            expenseGroup.SubItems.Any(expenseCategory => expenseCategory.Category == item)
        );
    }
}
