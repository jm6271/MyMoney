using MyMoney.Models;

namespace MyMoney.ViewModels.Windows
{
    public partial class BudgetCategoryEditorWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _WindowTitle = "Budget Category Editor";

        [ObservableProperty]
        private string _BudgetCategory = "";

        [ObservableProperty]
        private Currency _BudgetAmount = new(0m);

        public void CancelButtonClick()
        {

        }

        public void OkButtonClick()
        {

        }
    }
}
