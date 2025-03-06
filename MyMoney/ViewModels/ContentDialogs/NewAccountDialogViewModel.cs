using MyMoney.Core.FS.Models;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewAccountDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _accountName = "";

        [ObservableProperty]
        private Currency _startingBalance = new(0m);
    }
}
