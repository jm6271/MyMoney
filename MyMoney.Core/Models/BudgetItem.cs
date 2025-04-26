using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Models
{
    public partial class BudgetItem : ObservableObject
    {
        [ObservableProperty]
        private int _id = 0;

        [ObservableProperty]
        private string _category = "";

        [ObservableProperty]
        private Currency _amount = new(0m);
    }
}
