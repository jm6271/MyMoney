using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Models
{
    public partial class BudgetReportItem : ObservableObject
    {
        [ObservableProperty]
        private int _Id = 0;

        [ObservableProperty]
        private string _Category = "";

        [ObservableProperty]
        private Currency _Budgeted = new(0m);

        [ObservableProperty]
        private Currency _Actual = new(0m);

        [ObservableProperty]
        private Currency _Remaining = new(0m);
    }
}
