using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Models
{
    public partial class BudgetExpenseCategory : ObservableObject
    {
        [ObservableProperty]
        private int _id = 0;

        [ObservableProperty]
        private string _categoryName = "";

        [ObservableProperty]
        private ObservableCollection<BudgetItem> _subItems = [];

        public Currency CategoryTotal
        {
            get
            {
                decimal total = 0;
                foreach (var item in SubItems)
                {
                    total += Math.Abs(item.Amount.Value);
                }
                return new Currency(total);
            }
        }
    }
}
