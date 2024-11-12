using LiteDB;
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
        private int _ExpenseItemsSelectedIndex = 0;

        [ObservableProperty]
        private Currency _IncomeTotal = new(0m);

        [ObservableProperty]
        private Currency _ExpenseTotal = new(0m);

        [ObservableProperty]
        private decimal _ExpensePercentTotal = 0;

        public BudgetViewModel()
        {
            // Read the budget items from the database and populate the list views
            using(var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath()))
            {
                var incomeCollection = db.GetCollection<BudgetIncomeItem>("BudgetIncomeItems");
                var expenseCollection = db.GetCollection<BudgetExpenseItem>("BudgetExpenseItems");

                // load the income items collection
                for (int i = 1; i <= incomeCollection.Count(); i++)
                {
                    IncomeLineItems.Add(incomeCollection.FindById(i));
                }

                // Load the expense items collection
                for (int i = 1; i <= expenseCollection.Count(); i++)
                {
                    ExpenseLineItems.Add(expenseCollection.FindById(i));
                }
            }

            UpdateListViewTotals();
        }

        private void WriteToDatabase()
        {
            using(var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath()))
            {
                var incomeCollection = db.GetCollection<BudgetIncomeItem>("BudgetIncomeItems");
                var expenseCollection = db.GetCollection<BudgetExpenseItem>("BudgetExpenseItems");

                // clear the collections
                incomeCollection.DeleteAll();
                expenseCollection.DeleteAll();

                // add the new items to the database
                foreach (var item in IncomeLineItems)
                {
                    incomeCollection.Insert(item);
                }

                foreach (var item in ExpenseLineItems)
                {
                    expenseCollection.Insert(item);
                }
            }
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

            // write the items to the database
            WriteToDatabase();
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

                // Recalculate the total of the income items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private async Task DeleteIncomeItem()
        {
            // Show message box asking user if they really want to delete the category
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Delete Category?",
                Content = "Are you sure you want to delete the selected Category?",
                IsPrimaryButtonEnabled = false,
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = "Yes",
                CloseButtonText = "No",
                CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
            };

            var result = await uiMessageBox.ShowDialogAsync();

            if (result != Wpf.Ui.Controls.MessageBoxResult.Secondary) return; // User clicked no
            IncomeLineItems.RemoveAt(IncomeItemsSelectedIndex);

            // replace the id property of the remaining elements so the IDs are in a concecutive order (We have all kinds of problems when we don't do this)
            for (int i = 0; i < IncomeLineItems.Count; i++)
            {
                IncomeLineItems[i].Id = i + 1;
            }

            UpdateListViewTotals();
        }

        [RelayCommand]
        private void EditExpenseItem()
        {
            BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);
            editorWindowViewModel.BudgetCategory = ExpenseLineItems[ExpenseItemsSelectedIndex].Category;
            editorWindowViewModel.BudgetAmount = ExpenseLineItems[ExpenseItemsSelectedIndex].Amount;

            if (editorWindow.ShowDialog() == true)
            {
                // modify the item at the selected index
                BudgetExpenseItem expenseItem = new();
                expenseItem.Category = editorWindowViewModel.BudgetCategory;
                expenseItem.Amount = editorWindowViewModel.BudgetAmount;

                // assign the selected index of the list with the new item
                ExpenseLineItems[ExpenseItemsSelectedIndex] = expenseItem;

                // Recalculate the total of the expense items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private async Task DeleteExpenseItem()
        {
            // Show message box asking user if they really want to delete the category
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Delete Category?",
                Content = "Are you sure you want to delete the selected Category?",
                IsPrimaryButtonEnabled = false,
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = "Yes",
                CloseButtonText = "No",
                CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
            };

            var result = await uiMessageBox.ShowDialogAsync();

            if (result != Wpf.Ui.Controls.MessageBoxResult.Secondary) return; // User clicked no
            ExpenseLineItems.RemoveAt(ExpenseItemsSelectedIndex);

            // replace the id property of the remaining elements so the IDs are in a concecutive order (We have all kinds of problems when we don't do this)
            for (int i = 0; i < ExpenseLineItems.Count; i++)
            {
                ExpenseLineItems[i].Id = i + 1;
            }

            UpdateListViewTotals();
        }
    }
}
