using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class Account : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private ObservableCollection<Transaction> _transactions = [];

        [ObservableProperty]
        private string _accountName = "";

        [ObservableProperty]
        private Currency _total = new(0m);
    }
}
