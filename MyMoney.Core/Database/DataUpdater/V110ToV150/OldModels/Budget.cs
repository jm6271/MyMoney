using System.Collections.ObjectModel;
using MyMoney.Core.Models;

namespace MyMoney.DataUpdater.V110ToV150.OldModels;

internal sealed class Budget
{
    public int Id { get; set; }
    public string BudgetTitle { get; set; } = "";
    public DateTime BudgetDate { get; set; }
    public ObservableCollection<BudgetItem> BudgetIncomeItems { get; set; } = [];
    public ObservableCollection<BudgetExpenseCategory> BudgetExpenseItems { get; set; } = [];
    public ObservableCollection<BudgetSavingsCategory> BudgetSavingsCategories { get; set; } = [];
}

internal sealed class BudgetSavingsCategory
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = "";
    public Currency CurrentBalance { get; set; } = new(0m);
    public Currency BudgetedAmount { get; set; } = new(0m);
    public Currency Spent { get; set; } = new(0m);
    public string PlannedTransactionHash { get; set; } = "";
    public string BalanceTransactionHash { get; set; } = "";
    public ObservableCollection<Transaction> Transactions { get; set; } = [];
    public string CategoryHash { get; set; } = "";
}
