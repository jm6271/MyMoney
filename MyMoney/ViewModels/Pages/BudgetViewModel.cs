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
        private int _IncomeItemsSelectedIndex = 0;

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

            if(editorWindow.ShowDialog() == true)
            {
                // Create a new income item with the results from the dialog
                BudgetIncomeItem item = new();
                item.Category = editorWindowViewModel.BudgetCategory;
                item.Amount = editorWindowViewModel.BudgetAmount;

                // Add the item to the budget income items list
                IncomeLineItems.Add(item);

                // Recalculate the total of the income items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private void AddExpenseItem()
        {
            BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);

            if (editorWindow.ShowDialog() == true)
            {
                // Create a new expense item with the results from the dialog
                BudgetExpenseItem item = new();
                item.Category = editorWindowViewModel.BudgetCategory;
                item.Amount = editorWindowViewModel.BudgetAmount;

                // Add the item to the budget expense items list
                ExpenseLineItems.Add(item);

                // Recalculate the total of the expense items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private void EditIncomeItem()
        {
            BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);
            editorWindowViewModel.BudgetCategory = IncomeLineItems[IncomeItemsSelectedIndex].Category;
            editorWindowViewModel.BudgetAmount = IncomeLineItems[IncomeItemsSelectedIndex].Amount;

            if (editorWindow.ShowDialog() == true)
            {
                // modify the item at the selected index
                BudgetIncomeItem incomeItem = new();
                incomeItem.Category = editorWindowViewModel.BudgetCategory;
                incomeItem.Amount = editorWindowViewModel.BudgetAmount;

                // assign the selected index of the list with the new item
                IncomeLineItems[IncomeItemsSelectedIndex] = incomeItem;

                // Recalculate the total of the expense items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private void DeleteIncomeItem()
        {
            IncomeLineItems.RemoveAt(IncomeItemsSelectedIndex);
        }
    }
}
