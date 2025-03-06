using MyMoney.Core.FS.Models;
using System.Collections.ObjectModel;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class TransferDialogViewModel(ObservableCollection<string> accountNames) : ObservableObject
    {
        public ObservableCollection<string> Accounts { get; } = accountNames;

        [ObservableProperty]
        private string _transferFrom = "";

        [ObservableProperty]
        private string _transferTo = "";

        [ObservableProperty]
        private Currency _amount = new(0m);
    }
}
