using MyMoney.Core.Models;

namespace MyMoney.ViewModels.Windows
{
    public partial class NewAccountWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _AccountName = "";

        [ObservableProperty]
        private Currency _StartingBalance = new(0m);
    }
}
