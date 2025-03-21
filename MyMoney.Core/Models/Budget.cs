using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

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
        private ObservableCollection<BudgetItem> _budgetExpenseItems = [];
    }
}
