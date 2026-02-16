using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.DataUpdater.V100ToV110.OldModels
{
    internal partial class Account : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private ObservableCollection<Transaction> _transactions = [];

        [ObservableProperty]
        private string _accountName = "";

        [ObservableProperty]
        private Core.Models.Currency _total = new(0m);
    }
}
