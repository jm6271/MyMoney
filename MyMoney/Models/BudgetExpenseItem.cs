﻿namespace MyMoney.Models
{
    public partial class BudgetExpenseItem : ObservableObject
    {
        [ObservableProperty]
        private int _Id = 0;

        [ObservableProperty]
        private string _Category = "";

        [ObservableProperty]
        private Currency _Amount = new(0m);
    }
}
