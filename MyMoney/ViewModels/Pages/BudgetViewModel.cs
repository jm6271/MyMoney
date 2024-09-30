using MyMoney.Models;
using MyMoney.ViewModels.Windows;
using MyMoney.Views.Windows;
using System.Collections.ObjectModel;

namespace MyMoney.ViewModels.Pages
{
    public partial class BudgetViewModel : ObservableObject
    {
        public ObservableCollection<BudgetIncomeItem> IncomeLineItems { get; set; } = [];
        public ObservableCollection<BudgetExpenseItem> ExpenseLineItems { get; set; } = [];

        [ObservableProperty]
        private Currency _IncomeTotal = new(0m);

        [ObservableProperty]
        private Currency _ExpenseTotal = new(0m);

        [ObservableProperty]
        private decimal _ExpensePercentTotal = 0;

        public BudgetViewModel()
        {
            BudgetExpenseItem item1 = new();
            item1.Category = "Gas";
            item1.Amount = new(120m);

            BudgetExpenseItem item2 = new();
            item2.Category = "Misc.";
            item2.Amount = new(50m);

            ExpenseLineItems.Add(item1);
            ExpenseLineItems.Add(item2);

            BudgetIncomeItem incomeitem1 = new();
            incomeitem1.Amount = new(1200m);
            incomeitem1.Category = "Work";

            IncomeLineItems.Add(incomeitem1);

            UpdateListViewTotals();
        }

        public void UpdateListViewTotals()
        {
            // calculate the total income items
            IncomeTotal = new(0m);

            foreach (var item in IncomeLineItems)
            {
                IncomeTotal += item.Amount;
            }

            // Calculate the total expense items
            ExpenseTotal = new(0m);

            foreach (var item in ExpenseLineItems)
            {
                ExpenseTotal += item.Amount;
            }
        }

        [RelayCommand]
        private void AddIncomeItem()
        {
            BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);

            editorWindow.ShowDialog();
        }

        [RelayCommand]
        private void AddExpenseItem()
        {
            BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);

            editorWindow.ShowDialog();
        }
    }
}
