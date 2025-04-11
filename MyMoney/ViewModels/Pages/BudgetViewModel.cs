using System.Collections.ObjectModel;
using MyMoney.Core.Models;
using MyMoney.Core.Database;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;
using Wpf.Ui.Appearance;
using System.Globalization;
using System.ComponentModel;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui;
using MyMoney.Services.ContentDialogs;

namespace MyMoney.ViewModels.Pages
{
    public partial class BudgetViewModel : ObservableObject
    {
        private ObservableCollection<Budget> Budgets { get; } = [];

        // Collections for different groups of budgets
        public ObservableCollection<Budget> OldBudgets { get; } = [];
        public ObservableCollection<Budget> CurrentBudgets { get; } = [];
        public ObservableCollection<Budget> FutureBudgets { get; } = [];

        [ObservableProperty]
        private int _oldBudgetsSelectedIndex = -1;

        [ObservableProperty]
        private int _currentBudgetsSelectedIndex = -1;

        [ObservableProperty]
        private int _futureBudgetsSelectedIndex = -1;

        [ObservableProperty]
        private Budget? _currentBudget;

        [ObservableProperty]
        private ISeries[] _incomePercentagesSeries  = [];

        [ObservableProperty]
        private ISeries[] _expensePercentagesSeries  = [];

        [ObservableProperty]
        private LabelVisual _incomePercentagesTitle = new()
        {
            Text = "Income",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        [ObservableProperty]
        private LabelVisual _expensePercentagesTitle = new()
        {
            Text = "Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        // Colors for chart text (changes in light and dark modes)
        [ObservableProperty]
        private SKColor _chartTextColor = new(0x33, 0x33, 0x33);

        [ObservableProperty]
        private int _incomeItemsSelectedIndex;

        [ObservableProperty]
        private int _expenseItemsSelectedIndex;

        [ObservableProperty]
        private Currency _incomeTotal = new(0m);

        [ObservableProperty]
        private Currency _expenseTotal = new(0m);

        [ObservableProperty]
        private decimal _expensePercentTotal;

        [ObservableProperty]
        private bool _isEditingEnabled = true;

        // Content dialog service
        private readonly IContentDialogService _contentDialogService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly INewBudgetDialogService _newBudgetDialogService;
        private readonly IBudgetCategoryDialogService _budgetCategoryDialogService;
        private readonly INewExpenseGroupDialogService _newExpenseGroupDialogService;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(OldBudgetsSelectedIndex) when OldBudgetsSelectedIndex != -1:
                {
                    CurrentBudgetsSelectedIndex = -1;
                    FutureBudgetsSelectedIndex = -1;

                    // Find this budget in the budgets collection and load it
                    var index = FindBudgetIndex(OldBudgets[OldBudgetsSelectedIndex].BudgetTitle);
                    if (index != -1)
                    {
                        LoadBudget(index);
                    }

                    break;
                }
                case nameof(CurrentBudgetsSelectedIndex) when CurrentBudgetsSelectedIndex != -1:
                {
                    OldBudgetsSelectedIndex = -1;
                    FutureBudgetsSelectedIndex = -1;

                    // Find this budget in the budgets collection and load it
                    var index = FindBudgetIndex(CurrentBudgets[CurrentBudgetsSelectedIndex].BudgetTitle);
                    if (index != -1)
                    {
                        LoadBudget(index);
                    }

                    break;
                }
                case nameof(FutureBudgetsSelectedIndex) when FutureBudgetsSelectedIndex != -1:
                {
                    OldBudgetsSelectedIndex = -1;
                    CurrentBudgetsSelectedIndex = -1;

                    // Find this budget in the budgets collection and load it
                    var index = FindBudgetIndex(FutureBudgets[FutureBudgetsSelectedIndex].BudgetTitle);
                    if (index != -1)
                    {
                        LoadBudget(index);
                    }

                    break;
                }
            }
        }

        public BudgetViewModel(IContentDialogService contentDialogService, IDatabaseReader databaseReader,
            IMessageBoxService messageBoxService, INewBudgetDialogService newBudgetDialogService,
            IBudgetCategoryDialogService budgetCategoryDialogService, INewExpenseGroupDialogService newExpenseGroupDialogService)
        { 
            _contentDialogService = contentDialogService;
            _messageBoxService = messageBoxService;
            _newBudgetDialogService = newBudgetDialogService;
            _budgetCategoryDialogService = budgetCategoryDialogService;
            _newExpenseGroupDialogService = newExpenseGroupDialogService;

            var budgetCollection = databaseReader.GetCollection<Budget>("Budgets");

            foreach (var budget in budgetCollection.OfType<Budget>())
            {
                Budgets.Add(budget);
            }

            // figure out which budget is there for the current month and display it
            // Budgets are stored with a key that is the month name, followed by the year

            // look for current month
            var dt = DateTime.Now;
            var key = dt.ToString("MMMM, yyyy", CultureInfo.InvariantCulture);

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
                ExpenseTotal += item.CategoryTotal;
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
            var i = 0;
            foreach (var item in incomeTotals)
            {
                IncomePercentagesSeries[i] = new PieSeries<double> { Values = [item.Value], Name = item.Key };
                i++;
            }

            Dictionary<string, double> expenseTotals = [];
            foreach (var item in CurrentBudget.BudgetExpenseItems)
            {
                expenseTotals.Add(item.CategoryName, (double)item.CategoryTotal.Value);
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
            ChartTextColor = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light ? new SKColor(0x33, 0x33, 0x33) : new SKColor(0xff, 0xff, 0xff);

            IncomePercentagesTitle.Paint = new SolidColorPaint(ChartTextColor);
            ExpensePercentagesTitle.Paint = new SolidColorPaint(ChartTextColor);
        }

        [RelayCommand]
        private async Task AddIncomeItem()
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            var viewModel = new BudgetCategoryDialogViewModel();
            _budgetCategoryDialogService.SetViewModel(viewModel);
            var result = await _budgetCategoryDialogService.ShowDialogAsync(_contentDialogService, "New Income Item");
            viewModel = _budgetCategoryDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Create a new income item with the results from the dialog
                BudgetItem item = new()
                {
                    Category = viewModel.BudgetCategory,
                    Amount = viewModel.BudgetAmount
                };

                // Add the item to the budget income items list
                CurrentBudget.BudgetIncomeItems.Add(item);

                // Recalculate the total of the income items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private async Task AddExpenseGroup()
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            var viewModel = new NewExpenseGroupDialogViewModel();
            _newExpenseGroupDialogService.SetViewModel(viewModel);
            var result = await _newExpenseGroupDialogService.ShowDialogAsync(_contentDialogService, "New Expense Group");
            viewModel = _newExpenseGroupDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Add a new expense group
                BudgetExpenseCategory expenseGroup = new();
                expenseGroup.CategoryName = viewModel.GroupName;
                CurrentBudget.BudgetExpenseItems.Add(expenseGroup);

                // Update totals and write to database
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private async Task AddExpenseItem(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            var viewModel = new BudgetCategoryDialogViewModel();
            _budgetCategoryDialogService.SetViewModel(viewModel);
            var result = await _budgetCategoryDialogService.ShowDialogAsync(_contentDialogService, "New Expense Item");
            viewModel = _budgetCategoryDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Create a new expense item with the results from the dialog
                BudgetItem item = new()
                {
                    Category = viewModel.BudgetCategory,
                    Amount = viewModel.BudgetAmount
                };

                // Add the item to the budget expense items list
                parameter.SubItems.Add(item);

                // Recalculate the total of the expense items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private async Task EditIncomeItem()
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            var viewModel = new BudgetCategoryDialogViewModel
            {
                BudgetCategory = CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Category,
                BudgetAmount = CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Amount
            };

            _budgetCategoryDialogService.SetViewModel(viewModel);
            var result = await _budgetCategoryDialogService.ShowDialogAsync(_contentDialogService, "Edit Income Item");
            viewModel = _budgetCategoryDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // modify the item at the selected index
                BudgetItem incomeItem = new()
                {
                    Category = viewModel.BudgetCategory,
                    Amount = viewModel.BudgetAmount
                };

                // assign the selected index of the list with the new item
                CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex] = incomeItem;

                // Recalculate the total of the income items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private async Task DeleteIncomeItem()
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;


            // Show message box asking user if they really want to delete the category
            var result = await _messageBoxService.ShowAsync("Delete Category?",
                                                "Are you sure you want to delete the selected category?",
                                                "Yes",
                                                "No");

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary) return; // User clicked no

            CurrentBudget.BudgetIncomeItems.RemoveAt(IncomeItemsSelectedIndex);

            // replace the id property of the remaining elements so the IDs are in a consecutive order (We have all kinds of problems when we don't do this)
            for (var i = 0; i < CurrentBudget.BudgetIncomeItems.Count; i++)
            {
                CurrentBudget.BudgetIncomeItems[i].Id = i + 1;
            }

            UpdateListViewTotals();
        }

        [RelayCommand]
        private async Task EditExpenseGroup(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            var viewModel = new NewExpenseGroupDialogViewModel
            {
                GroupName = parameter.CategoryName,
            };

            _newExpenseGroupDialogService.SetViewModel(viewModel);
            var result = await _newExpenseGroupDialogService.ShowDialogAsync(_contentDialogService, "Edit Group Name");
            viewModel = _newExpenseGroupDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                parameter.CategoryName = viewModel.GroupName;
            }
        }

        [RelayCommand]
        private async Task EditExpenseItem(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            var viewModel = new BudgetCategoryDialogViewModel
            {
                BudgetCategory = parameter.SubItems[parameter.SelectedSubItemIndex].Category,
                BudgetAmount = parameter.SubItems[parameter.SelectedSubItemIndex].Amount,
            };

            _budgetCategoryDialogService.SetViewModel(viewModel);
            var result = await _budgetCategoryDialogService.ShowDialogAsync(_contentDialogService, "Edit Expense Item");
            viewModel = _budgetCategoryDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // modify the item at the selected index
                BudgetItem expenseItem = new()
                {
                    Category = viewModel.BudgetCategory,
                    Amount = viewModel.BudgetAmount
                };

                // assign the selected index of the list with the new item
                parameter.SubItems[parameter.SelectedSubItemIndex] = expenseItem;

                // Recalculate the total of the expense items
                UpdateListViewTotals();
            }
        }

        [RelayCommand]
        private async Task DeleteExpenseGroup(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            // as user if they really want to delete the group
            var result = await _messageBoxService.ShowAsync("Delete Group?",
                "Are you sure you want to delete the selected category?",
                "Yes",
                "No");

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary) return; // User clicked no

            // delete the item
            CurrentBudget.BudgetExpenseItems.Remove(parameter);
        }

        [RelayCommand]
        private async Task DeleteExpenseItem(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            // Show message box asking user if they really want to delete the category
            var result = await _messageBoxService.ShowAsync("Delete Category?",
                                                "Are you sure you want to delete the selected category?",
                                                "Yes",
                                                "No");

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary) return; // User clicked no

            parameter.SubItems.RemoveAt(parameter.SelectedSubItemIndex);

            // replace the id property of the remaining elements so the IDs are
            // in a consecutive order (We have all kinds of problems when we don't do this)
            for (int i = 0; i < parameter.SubItems.Count; i++)
            {
                parameter.SubItems[i].Id = i + 1;
            }

            UpdateListViewTotals();
        }

        [RelayCommand]
        private async Task CreateNewBudget()
        {
            // Create the new budget dialog
            var viewModel = new NewBudgetDialogViewModel();
            _newBudgetDialogService.SetViewModel(viewModel);
            var result = await _newBudgetDialogService.ShowDialogAsync(_contentDialogService);

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // make sure this budget doesn't exist already
                if (DoesBudgetExist(Convert.ToDateTime(viewModel.SelectedDate)))
                {
                    // warn user that the budget exists
                    await _messageBoxService.ShowInfoAsync("Budget Already Exists",
                            "Cannot create a budget for the selected month because a budget for this month already exists",
                            "OK");
                    return;
                }

                // Add the budget
                Budget newBudget = new();

                var budgetTitle = viewModel.SelectedDate;
                newBudget.BudgetTitle = budgetTitle;
                newBudget.BudgetDate = Convert.ToDateTime(budgetTitle);

                // Copy over categories if box is checked
                if (viewModel.UseLastMonthsBudget && CurrentBudget != null)
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

                // Update budget lists
                UpdateBudgetLists();

                // Set as current budget
                SetCurrentBudget(budgetTitle);
            }
        }

        private void SetCurrentBudget(string budgetTitle)
        {
            foreach (var item in Budgets)
            {
                if (item.BudgetTitle != budgetTitle) continue;
                CurrentBudget = item;

                // Select listview item for this budget item
                if (item.BudgetDate.Month == DateTime.Now.Month)
                {
                    CurrentBudgetsSelectedIndex = 0;
                    OldBudgetsSelectedIndex = -1;
                    FutureBudgetsSelectedIndex = -1;
                }
                else
                {
                    CurrentBudgetsSelectedIndex = -1;
                    OldBudgetsSelectedIndex = -1;
                    FutureBudgetsSelectedIndex = 0;
                }
                break;
            }
        }

        private bool DoesBudgetExist(DateTime selectedDate)
        {
            foreach (var budget in Budgets)
            {
                if (budget.BudgetDate.ToString("MMMM, yyyy") == selectedDate.ToString("MMMM, yyyy"))
                {
                    return true;
                }
            }

            return false;
        }

        private void LoadBudget(int index)
        {
            // Load into current budget
            CurrentBudget = Budgets[index];

            IsEditingEnabled = CurrentBudget.BudgetDate > DateTime.Now.AddMonths(-1);

            UpdateCharts();
        }

        private int FindBudgetIndex(string budgetName)
        {
            for (var i = 0; i < Budgets.Count; i++)
            {
                if (Budgets[i].BudgetTitle == budgetName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
