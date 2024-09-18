using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Models
{
    public partial class BudgetIncomeItem : ObservableObject
    {
        [ObservableProperty]
        private string _Category = "";

        [ObservableProperty]
        private Currency _Amount = new(0m);

    }
}
