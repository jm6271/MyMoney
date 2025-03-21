using MyMoney.Core.Models;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class BudgetCategoryDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _budgetCategory = "";

        [ObservableProperty]
        private Currency _budgetAmount = new(0m);
    }
}
