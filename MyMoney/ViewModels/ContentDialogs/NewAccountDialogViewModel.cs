using MyMoney.Core.FS.Models;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewAccountDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _AccountName = "";

        [ObservableProperty]
        private Currency _StartingBalance = new(0m);
    }
}
