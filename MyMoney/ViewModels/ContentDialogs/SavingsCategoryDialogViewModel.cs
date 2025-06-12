using MyMoney.Core.Models;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class SavingsCategoryDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private Currency _planned = new(0m);

        [ObservableProperty]
        private Currency _currentBalance = new(0m);
    }
}
