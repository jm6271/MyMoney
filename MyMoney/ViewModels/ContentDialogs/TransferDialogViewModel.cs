using MyMoney.Core.FS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class TransferDialogViewModel(ObservableCollection<string> AccountNames) : ObservableObject
    {
        public ObservableCollection<string> Accounts { get; set; } = AccountNames;

        [ObservableProperty]
        private string _TransferFrom = "";

        [ObservableProperty]
        private string _TransferTo = "";

        [ObservableProperty]
        private Currency _Amount = new(0m);
    }
}
