using MyMoney.ViewModels.Windows;
using MyMoney.Views.Windows;
using System.Collections.ObjectModel;
using MyMoney.Core.Models;
using MyMoney.Core.Database;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;
using Wpf.Ui.Appearance;

namespace MyMoney.ViewModels.Pages
{
    public partial class BudgetViewModel : ObservableObject
    {
        public ObservableCollection<BudgetIncomeItem> IncomeLineItems { get; set; } = [];
        public ObservableCollection<BudgetExpenseItem> ExpenseLineItems { get; set; } = [];

        public ISeries[] IncomePercentagesSeries { get; set; } = [];
        public ISeries[] ExpensePercentagesSeries { get; set; } = [];

        [ObservableProperty]
        private LabelVisual _IncomePercentages_Title = new LabelVisual
        {
            Text = "Income",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        [ObservableProperty]
        private LabelVisual _ExpensePercentages_Title = new LabelVisual
        {
            Text = "Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        // Colors for chart text (changes in light and dark modes)
        [ObservableProperty]
        private SKColor _ChartTextColor = new(0x33, 0x33, 0x33);

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

        [ObservableProperty]
        private string _BudgetTitle = "Budget";

        public BudgetViewModel()
        {
            // Read the budget items from the database and populate the list views

            var incomeCollection = DatabaseReader.GetCollection<BudgetIncomeItem>("BudgetIncomeItems");
            var expenseCollection = DatabaseReader.GetCollection<BudgetExpenseItem>("BudgetExpenseItems");

            foreach (var item in incomeCollection)
            {
                IncomeLineItems.Add(item);
            }

            foreach (var item in expenseCollection)
            {
                ExpenseLineItems.Add(item);
            }

            UpdateListViewTotals();
        }

        public void OnPageNavigatedTo()
        {
            UpdateCharts();
        }

        private void WriteToDatabase()
        {
            DatabaseWriter.WriteCollection("BudgetIncomeItems", [.. IncomeLineItems]);
            DatabaseWriter.WriteCollection("BudgetExpenseItems", [.. ExpenseLineItems]);
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

            // Update the charts
            UpdateCharts();
        }

        private void UpdateCharts()
        {
            Dictionary<string, double> incomeTotals = [];
            foreach (var item in IncomeLineItems)
            {
                incomeTotals.Add(item.Category, (double)item.Amount.Value);
            }

            IncomePercentagesSeries = new ISeries[incomeTotals.Count];
            int i = 0;
            foreach (var item in incomeTotals)
            {
                IncomePercentagesSeries[i] = new PieSeries<double> { Values = [item.Value], Name = item.Key };
                i++;
            }

            Dictionary<string, double> expenseTotals = [];
            foreach (var item in ExpenseLineItems)
            {
                expenseTotals.Add(item.Category, (double)item.Amount.Value);
            }

            ExpensePercentagesSeries = new ISeries[expenseTotals.Count];
            i = 0;
            foreach (var item in expenseTotals)
            {
                ExpensePercentagesSeries[i] = new PieSeries<double> { Values = [item.Value], Name = item.Key };
                i++;
            }

            // Update theme
            UpdateChartTheme();
        }

        private void UpdateChartTheme()
        {
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light)
            {
                ChartTextColor = new SKColor(0x33, 0x33, 0x33);
            }
            else
            {
                ChartTextColor = new SKColor(0xff, 0xff, 0xff);
            }

            IncomePercentages_Title.Paint = new SolidColorPaint(ChartTextColor);
            ExpensePercentages_Title.Paint = new SolidColorPaint(ChartTextColor);
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
                item.Amount = new(editorWindowViewModel.BudgetAmount);

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
                item.Amount = new(editorWindowViewModel.BudgetAmount);

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
            editorWindowViewModel.BudgetAmount = IncomeLineItems[IncomeItemsSelectedIndex].Amount.Value;

            if (editorWindow.ShowDialog() == true)
            {
                // modify the item at the selected index
                BudgetIncomeItem incomeItem = new();
                incomeItem.Category = editorWindowViewModel.BudgetCategory;
                incomeItem.Amount = new(editorWindowViewModel.BudgetAmount);

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
            editorWindowViewModel.BudgetAmount = ExpenseLineItems[ExpenseItemsSelectedIndex].Amount.Value;

            if (editorWindow.ShowDialog() == true)
            {
                // modify the item at the selected index
                BudgetExpenseItem expenseItem = new();
                expenseItem.Category = editorWindowViewModel.BudgetCategory;
                expenseItem.Amount = new(editorWindowViewModel.BudgetAmount);

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

        [RelayCommand]
        private void CreateNewBudget()
        {
            // Show the new budget dialog
            NewBudgetWindowViewModel viewModel = new();
            NewBudgetDialog dlg = new(viewModel);

            dlg.Owner = Application.Current.MainWindow;

            if (dlg.ShowDialog() == true) 
            { 

            }
        }
    }
}
