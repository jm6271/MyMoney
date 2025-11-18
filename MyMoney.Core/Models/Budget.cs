using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class Budget : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _budgetTitle = "";

        [ObservableProperty]
        private DateTime _budgetDate;

        [ObservableProperty]
        private ObservableCollection<BudgetItem> _budgetIncomeItems = [];

        [ObservableProperty]
        private ObservableCollection<BudgetExpenseCategory> _budgetExpenseItems = [];

        [ObservableProperty]
        private ObservableCollection<BudgetSavingsCategory> _budgetSavingsCategories = [];
    }
}
