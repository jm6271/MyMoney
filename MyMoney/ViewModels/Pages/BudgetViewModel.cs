using MyMoney.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.ViewModels.Pages
{
    public partial class BudgetViewModel : ObservableObject
    {
        public ObservableCollection<BudgetIncomeItem> IncomeLineItems { get; set; } = [];
        public ObservableCollection<BudgetExpenseItem> ExpenseLineItems { get; set; } = [];

        [ObservableProperty]
        private Currency _IncomeTotal = new(0m);

        [ObservableProperty]
        private Currency _ExpenseTotal = new(0m);

        [ObservableProperty]
        private decimal _ExpensePercentTotal = 0;

        public BudgetViewModel()
        {
            BudgetExpenseItem item1 = new();
            item1.Category = "Gas";
            item1.Amount = new(120m);

            BudgetExpenseItem item2 = new();
            item2.Category = "Misc.";
            item2.Amount = new(50m);

            ExpenseLineItems.Add(item1);
            ExpenseLineItems.Add(item2);

            BudgetIncomeItem incomeitem1 = new();
            incomeitem1.Amount = new(1200m);
            incomeitem1.Category = "Work";

            IncomeLineItems.Add(incomeitem1);
        }
    }
}
