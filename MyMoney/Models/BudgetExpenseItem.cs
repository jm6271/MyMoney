using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Models
{
    public partial class BudgetExpenseItem : ObservableObject
    {
        [ObservableProperty]
        private string _Category = "";

        [ObservableProperty]
        private decimal _Percent = 0m;

        [ObservableProperty]
        private Currency _Amount = new(0m);
    }
}
