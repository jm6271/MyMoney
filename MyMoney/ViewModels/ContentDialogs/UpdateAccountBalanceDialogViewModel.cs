using MyMoney.Core.Models;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class UpdateAccountBalanceDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private Currency _balance = new(0m);
    }
}
