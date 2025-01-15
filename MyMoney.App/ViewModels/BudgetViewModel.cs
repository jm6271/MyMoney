using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMoney.Core.FS.Models;
using MyMoney.Core.Database;
using SkiaSharp;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.UI.Xaml.Controls;
using MyMoney.App.Views.ContentDialogs;
using MyMoney.App.ViewModels.ContentDialogs;

namespace MyMoney.App.ViewModels
{
    public partial class BudgetViewModel : ObservableObject
    {
        public ObservableCollection<Budget> Budgets { get; set; } = [];

        // Collections for different groups of budgets
        public ObservableCollection<Budget> OldBudgets { get; set; } = [];
        public ObservableCollection<Budget> CurrentBudgets { get; set; } = [];
        public ObservableCollection<Budget> FutureBudgets { get; set; } = [];

        [ObservableProperty]
        public partial int OldBudgetsSelectedIndex { get; set; } = -1;

        [ObservableProperty]
        public partial int CurrentBudgetsSelectedIndex { get; set; } = -1;

        [ObservableProperty]
        public partial int FutureBudgetsSelectedIndex { get; set; } = -1;

        [ObservableProperty]
        public partial Budget? CurrentBudget { get; set; } = null;

        [ObservableProperty]
        public partial ISeries[] IncomePercentagesSeries { get; set; } = [];

        [ObservableProperty]
        public partial ISeries[] ExpensePercentagesSeries { get; set; } = [];

        [ObservableProperty]
        public partial LabelVisual IncomePercentages_Title { get; set; } = new LabelVisual
        {
            Text = "Income",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        [ObservableProperty]
        public partial LabelVisual ExpensePercentages_Title { get; set; } = new LabelVisual
        {
            Text = "Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        // Colors for chart text (changes in light and dark modes)
        [ObservableProperty]
        public partial SKColor ChartTextColor { get; set; } = new(0x33, 0x33, 0x33);

        [ObservableProperty]
        public partial int IncomeItemsSelectedIndex { get; set; } = 0;

        [ObservableProperty]
        public partial int ExpenseItemsSelectedIndex { get; set; } = 0;

        [ObservableProperty]
        public partial Currency IncomeTotal { get; set; } = new(0m);

        [ObservableProperty]
        public partial Currency ExpenseTotal { get; set; } = new(0m);

        [ObservableProperty]
        public partial decimal ExpensePercentTotal { get; set; } = 0;

        [ObservableProperty]
        public partial bool IsEditingEnabled { get; set; } = true;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(OldBudgetsSelectedIndex) && OldBudgetsSelectedIndex != -1)
            {
                CurrentBudgetsSelectedIndex = -1;
                FutureBudgetsSelectedIndex = -1;

                // Find this budget in the budgets collection and load it
                var index = FindBudgetIndex(OldBudgets[OldBudgetsSelectedIndex].BudgetTitle);
                if (index != -1)
                {
                    LoadBudget(index);
                }
            }
            else if (e.PropertyName == nameof(CurrentBudgetsSelectedIndex) && CurrentBudgetsSelectedIndex != -1)
            {
                OldBudgetsSelectedIndex = -1;
                FutureBudgetsSelectedIndex = -1;

                // Find this budget in the budgets collection and load it
                var index = FindBudgetIndex(CurrentBudgets[CurrentBudgetsSelectedIndex].BudgetTitle);
                if (index != -1)
                {
                    LoadBudget(index);
                }
            }
            else if (e.PropertyName == nameof(FutureBudgetsSelectedIndex) && FutureBudgetsSelectedIndex != -1)
            {
                OldBudgetsSelectedIndex = -1;
                CurrentBudgetsSelectedIndex = -1;

                // Find this budget in the budgets collection and load it
                var index = FindBudgetIndex(FutureBudgets[FutureBudgetsSelectedIndex].BudgetTitle);
                if (index != -1)
                {
                    LoadBudget(index);
                }
            }
        }

        public BudgetViewModel()
        {
            var budgetCollection = DatabaseReader.GetCollection<Budget>("Budgets");

            foreach (var budget in budgetCollection)
            {
                if (budget != null)
                {
                    Budgets.Add(budget);
                }
            }

            // figure out which budget is there for the current month and display it
            // Budgets are stored with a key that is the month name, followed by the year

            // look for current month
            DateTime dt = DateTime.Now;
            string key = dt.ToString("MMMM, yyyy", CultureInfo.InvariantCulture);

            foreach (var budget in Budgets)
            {
                if (budget.BudgetTitle == key)
                {
                    CurrentBudget = budget;
                }
            }

            if (CurrentBudget == null)
            {
                // No budget for the current month
                return;
            }

            UpdateListViewTotals();
        }

        public void OnPageNavigatedTo()
        {
            UpdateCharts();
            UpdateBudgetLists();

            // Select the current budget
            if (CurrentBudgets.Count == 1)
            {
                CurrentBudgetsSelectedIndex = 0;
            }
        }

        private void UpdateBudgetLists()
        {
            CurrentBudgets.Clear();
            OldBudgets.Clear();
            FutureBudgets.Clear();

            foreach (var budget in Budgets)
            {
                if (budget.BudgetDate.Month == DateTime.Now.Month && budget.BudgetDate.Year == DateTime.Now.Year)
                {
                    CurrentBudgets.Add(budget);
                }
                else if (budget.BudgetDate > DateTime.Now)
                {
                    FutureBudgets.Add(budget);
                }
                else
                {
                    // Don't add a budget from more than 1 year ago
                    if (budget.BudgetDate < DateTime.Now.AddYears(-1))
                        continue;

                    OldBudgets.Add(budget);
                }
            }
        }

        private void WriteToDatabase()
        {
            if (CurrentBudget == null) return;

            DatabaseWriter.WriteCollection("Budgets", Budgets.ToList());
        }

        public void UpdateListViewTotals()
        {
            if (CurrentBudget == null)
            {
                // set to zero
                IncomeTotal = new(0m);
                ExpenseTotal = new(0m);

                return;
            }

            // calculate the total income items
            IncomeTotal = new(0m);

            foreach (var item in CurrentBudget.BudgetIncomeItems)
            {
                IncomeTotal += item.Amount;
            }

            // Calculate the total expense items
            ExpenseTotal = new(0m);

            foreach (var item in CurrentBudget.BudgetExpenseItems)
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
            if (CurrentBudget == null)
                return;

            Dictionary<string, double> incomeTotals = [];
            foreach (var item in CurrentBudget.BudgetIncomeItems)
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
            foreach (var item in CurrentBudget.BudgetExpenseItems)
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
            if (Application.Current.RequestedTheme == ApplicationTheme.Light)
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
            //if (CurrentBudget == null) return;
            //if (!IsEditingEnabled) return;

            //BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            //BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);

            //if (editorWindow.ShowDialog() == true)
            //{
            //    // Create a new income item with the results from the dialog
            //    BudgetItem item = new();
            //    item.Category = editorWindowViewModel.BudgetCategory;
            //    item.Amount = new(editorWindowViewModel.BudgetAmount);

            //    // Add the item to the budget income items list
            //    CurrentBudget.BudgetIncomeItems.Add(item);

            //    // Recalculate the total of the income items
            //    UpdateListViewTotals();
            //}
        }

        [RelayCommand]
        private void AddExpenseItem()
        {
            //if (CurrentBudget == null) return;
            //if (!IsEditingEnabled) return;

            //BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            //BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);

            //if (editorWindow.ShowDialog() == true)
            //{
            //    // Create a new expense item with the results from the dialog
            //    BudgetItem item = new();
            //    item.Category = editorWindowViewModel.BudgetCategory;
            //    item.Amount = new(editorWindowViewModel.BudgetAmount);

            //    // Add the item to the budget expense items list
            //    CurrentBudget.BudgetExpenseItems.Add(item);

            //    // Recalculate the total of the expense items
            //    UpdateListViewTotals();
            //}
        }

        [RelayCommand]
        private void EditIncomeItem()
        {
            //if (CurrentBudget == null) return;
            //if (!IsEditingEnabled) return;

            //BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            //BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);
            //editorWindowViewModel.BudgetCategory = CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Category;
            //editorWindowViewModel.BudgetAmount = CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Amount.Value;

            //if (editorWindow.ShowDialog() == true)
            //{
            //    // modify the item at the selected index
            //    BudgetItem incomeItem = new();
            //    incomeItem.Category = editorWindowViewModel.BudgetCategory;
            //    incomeItem.Amount = new(editorWindowViewModel.BudgetAmount);

            //    // assign the selected index of the list with the new item
            //    CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex] = incomeItem;

            //    // Recalculate the total of the income items
            //    UpdateListViewTotals();
            //}
        }

        [RelayCommand]
        private void DeleteIncomeItem()
        {
            //if (CurrentBudget == null) return;
            //if (!IsEditingEnabled) return;

            //// Show message box asking user if they really want to delete the category
            //var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            //{
            //    Title = "Delete Category?",
            //    Content = "Are you sure you want to delete the selected Category?",
            //    IsPrimaryButtonEnabled = false,
            //    IsSecondaryButtonEnabled = true,
            //    SecondaryButtonText = "Yes",
            //    CloseButtonText = "No",
            //    CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
            //};

            //var result = await uiMessageBox.ShowDialogAsync();

            //if (result != Wpf.Ui.Controls.MessageBoxResult.Secondary) return; // User clicked no
            //CurrentBudget.BudgetIncomeItems.RemoveAt(IncomeItemsSelectedIndex);

            //// replace the id property of the remaining elements so the IDs are in a concecutive order (We have all kinds of problems when we don't do this)
            //for (int i = 0; i < CurrentBudget.BudgetIncomeItems.Count; i++)
            //{
            //    CurrentBudget.BudgetIncomeItems[i].Id = i + 1;
            //}

            //UpdateListViewTotals();
        }

        [RelayCommand]
        private void EditExpenseItem()
        {
            //if (CurrentBudget == null) return;
            //if (!IsEditingEnabled) return;

            //BudgetCategoryEditorWindowViewModel editorWindowViewModel = new();
            //BudgetCategoryEditorWindow editorWindow = new(editorWindowViewModel);
            //editorWindowViewModel.BudgetCategory = CurrentBudget.BudgetExpenseItems[ExpenseItemsSelectedIndex].Category;
            //editorWindowViewModel.BudgetAmount = CurrentBudget.BudgetExpenseItems[ExpenseItemsSelectedIndex].Amount.Value;

            //if (editorWindow.ShowDialog() == true)
            //{
            //    // modify the item at the selected index
            //    BudgetItem expenseItem = new();
            //    expenseItem.Category = editorWindowViewModel.BudgetCategory;
            //    expenseItem.Amount = new(editorWindowViewModel.BudgetAmount);

            //    // assign the selected index of the list with the new item
            //    CurrentBudget.BudgetExpenseItems[ExpenseItemsSelectedIndex] = expenseItem;

            //    // Recalculate the total of the expense items
            //    UpdateListViewTotals();
            //}
        }

        [RelayCommand]
        private void DeleteExpenseItem()
        {
            //if (CurrentBudget == null) return;
            //if (!IsEditingEnabled) return;

            //// Show message box asking user if they really want to delete the category
            //var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            //{
            //    Title = "Delete Category?",
            //    Content = "Are you sure you want to delete the selected Category?",
            //    IsPrimaryButtonEnabled = false,
            //    IsSecondaryButtonEnabled = true,
            //    SecondaryButtonText = "Yes",
            //    CloseButtonText = "No",
            //    CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
            //};

            //var result = await uiMessageBox.ShowDialogAsync();

            //if (result != Wpf.Ui.Controls.MessageBoxResult.Secondary) return; // User clicked no
            //CurrentBudget.BudgetExpenseItems.RemoveAt(ExpenseItemsSelectedIndex);

            //// replace the id property of the remaining elements so the IDs are in a concecutive order (We have all kinds of problems when we don't do this)
            //for (int i = 0; i < CurrentBudget.BudgetExpenseItems.Count; i++)
            //{
            //    CurrentBudget.BudgetExpenseItems[i].Id = i + 1;
            //}

            //UpdateListViewTotals();
        }

        public async Task CreateNewBudget(XamlRoot xamlRoot)
        {
            // Show the new budget dialog
            NewBudgetDialog dialog = new();
            dialog.XamlRoot = xamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "New Budget";
            dialog.PrimaryButtonText = "New";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Add a budget
                Budget newBudget = new();
          
                string budgetTitle = dialog.CmbBudgetDates.Text;
                newBudget.BudgetTitle = budgetTitle;
                newBudget.BudgetDate = Convert.ToDateTime(budgetTitle);

                // Copy over categories if box is checked
                if (dialog.ChkUseLastMonthBudget.IsChecked == true && CurrentBudget != null)
                {
                    foreach (var item in CurrentBudget.BudgetIncomeItems)
                    {
                        newBudget.BudgetIncomeItems.Add(item);
                    }

                    foreach (var item in CurrentBudget.BudgetExpenseItems)
                    {
                        newBudget.BudgetExpenseItems.Add(item);
                    }
                }

                // Add to list of budgets
                Budgets.Add(newBudget);

                // Set as current budget
                foreach (var item in Budgets)
                {
                    if (item.BudgetTitle == budgetTitle)
                    {
                        CurrentBudget = item;
                        break;
                    }
                }
            }
        }

        private void LoadBudget(int index)
        {
            // Load into current budget
            CurrentBudget = Budgets[index];

            if (CurrentBudget.BudgetDate <= DateTime.Now.AddMonths(-1))
                IsEditingEnabled = false;
            else
                IsEditingEnabled = true;

            UpdateCharts();
        }

        private int FindBudgetIndex(string BudgetName)
        {
            for (int i = 0; i < Budgets.Count; i++)
            {
                if (Budgets[i].BudgetTitle == BudgetName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
