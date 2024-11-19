using MyMoney.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.ViewModels.Windows
{
    public partial class TransferWindowViewModel : ObservableObject
    {
        public ObservableCollection<string> Accounts { get; set; } = [];

        public TransferWindowViewModel(ObservableCollection<string> AccountNames)
        {
            Accounts = AccountNames;
        }

        [ObservableProperty]
        private string _TransferFrom = "";

        [ObservableProperty]
        private string _TransferTo = "";

        [ObservableProperty]
        private Currency _Amount = new(0m);
    }
}
