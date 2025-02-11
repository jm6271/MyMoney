using MyMoney.Core.FS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class BudgetCategoryDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _BudgetCategory = "";

        [ObservableProperty]
        private Currency _BudgetAmount = new(0m);
    }
}
