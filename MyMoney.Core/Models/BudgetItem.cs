using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class BudgetItem : ObservableObject, ICloneable
    {
        public object Clone()
        {
            return new BudgetItem()
            {
                Id = this.Id,
                Category = this.Category,
                Amount = new Currency(this.Amount.Value),
                Actual = new Currency(this.Actual.Value),
            };
        }

        [ObservableProperty]
        private int _id = 0;

        [ObservableProperty]
        private string _category = "";

        [ObservableProperty]
        private Currency _amount = new(0m);

        [ObservableProperty]
        private Currency _actual = new(0m);
    }
}
