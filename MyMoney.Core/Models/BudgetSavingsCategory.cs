using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class BudgetSavingsCategory : ObservableObject, ICloneable
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
        private string _plannedTransactionHash = string.Empty;

        [ObservableProperty]
        private string _balanceTransactionHash = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Transaction> _transactions = [];

        public string CategoryHash { get; private set; } = "";

        public BudgetSavingsCategory()
        {
            if (CategoryHash == "")
            {
                // Generate a hash for this category
                CategoryHash = Guid.NewGuid().ToString();
            }
        }

        public object Clone()
        {
            return new BudgetSavingsCategory
            {
                CategoryName = this.CategoryName,
                CurrentBalance = new Currency(this.CurrentBalance.Value),
                Spent = new Currency(0.0m),
                BudgetedAmount = new(this.BudgetedAmount.Value),
                Transactions = [],
                CategoryHash = this.CategoryHash,
            };
        }
    }
}
