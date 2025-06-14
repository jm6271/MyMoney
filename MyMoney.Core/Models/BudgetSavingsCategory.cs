using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Models
{
    public partial class BudgetSavingsCategory : ObservableObject
    {
        [ObservableProperty]
        private int _id = 0;

        [ObservableProperty]
        private string _categoryName = string.Empty;

        [ObservableProperty]
        private Currency _currentBalance = new(0m);

        [ObservableProperty]
        private Currency _budgetedAmount = new(0m);

        [ObservableProperty]
        private Currency _spent = new(0m);

        [ObservableProperty]
        private ObservableCollection<Transaction> _transactions = [];
    }
}
