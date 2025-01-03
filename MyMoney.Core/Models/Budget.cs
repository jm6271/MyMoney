using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Models
{
    public partial class Budget : ObservableObject
    {
        [ObservableProperty]
        private int _Id = 0;

        public ObservableCollection<BudgetIncomeItem> BudgetIncomeItems { get; set; } = [];

        public ObservableCollection<BudgetExpenseItem> BudgetExpenseItems { get; set; } = [];

        [ObservableProperty]
        private string _BudgetTitle = string.Empty;

        [ObservableProperty]
        private DateTime _BudgetDate = DateTime.Now;
    }
}
