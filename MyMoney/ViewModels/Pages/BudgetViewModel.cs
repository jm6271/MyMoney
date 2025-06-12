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
using System.Linq.Expressions;
using MyMoney.Core.Reports;
using System.Linq;
using System.Windows.Data;

namespace MyMoney.ViewModels.Pages
{
    public partial class BudgetViewModel : ObservableObject
    {
        public ObservableCollection<Budget> Budgets { get; } = [];

        [ObservableProperty]
        private ListCollectionView _groupedBudgets = new(new List<GroupedBudget>());

        [ObservableProperty]
        private GroupedBudget? _selectedGroupedBudget;

        [ObservableProperty]
        private int _selectedGroupedBudgetIndex;

        [ObservableProperty]
        private Budget? _currentBudget;

        [ObservableProperty]
        private ISeries[] _incomePercentagesSeries = [];

        [ObservableProperty]
        private ISeries[] _expensePercentagesSeries = [];

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
        private Currency _leftToBudget = new(0m);

        [ObservableProperty]
        private decimal _expensePercentTotal;

        [ObservableProperty]
        private bool _isEditingEnabled = true;

        public class GroupedBudget
        {
            public string Group { get; set; } = "";
            public Budget Budget { get; set; } = new();
        }

        public class GroupComparer : System.Collections.IComparer
        {
            private readonly Dictionary<string, int> _groupOrder = new()
            {
                { "Current", 0 },
                { "Future", 1 },
                { "Past", 2 }
            };

            public int Compare(object? x, object? y)
            {
                if (x is not GroupedBudget group1 || y is not GroupedBudget group2)
                    return 0;

                string name1 = group1.Group;
                string name2 = group2.Group;

                // If the group names are in our dictionary, use the custom order
                if (_groupOrder.TryGetValue(name1, out int value) && _groupOrder.TryGetValue(name2, out int value2))
                {
                    return value.CompareTo(value2);
                }

                // Fall back to alphabetical sorting for any other groups
                return string.Compare(name1, name2);
            }
        }


        // Content dialog service
        private readonly IContentDialogService _contentDialogService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly INewBudgetDialogService _newBudgetDialogService;
        private readonly IBudgetCategoryDialogService _budgetCategoryDialogService;
        private readonly INewExpenseGroupDialogService _newExpenseGroupDialogService;
        private readonly ISavingsCategoryDialogService _savingsCategoryDialogService;
        private readonly IDatabaseReader _databaseReader;

        public BudgetViewModel(IContentDialogService contentDialogService, IDatabaseReader databaseReader,
            IMessageBoxService messageBoxService, INewBudgetDialogService newBudgetDialogService,
            IBudgetCategoryDialogService budgetCategoryDialogService, INewExpenseGroupDialogService newExpenseGroupDialogService,
            ISavingsCategoryDialogService savingsCategoryDialogService)
        {
            _contentDialogService = contentDialogService;
            _messageBoxService = messageBoxService;
            _newBudgetDialogService = newBudgetDialogService;
            _budgetCategoryDialogService = budgetCategoryDialogService;
            _newExpenseGroupDialogService = newExpenseGroupDialogService;
            _savingsCategoryDialogService = savingsCategoryDialogService;
            _databaseReader = databaseReader;

            var budgetCollection = _databaseReader.GetCollection<Budget>("Budgets");

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
            UpdateListViewTotals();

            // Select the current budget
            if (Budgets.Count > 0)
            {
                SelectedGroupedBudgetIndex = 0;
            }
        }

        private void UpdateBudgetLists()
        {
            List<GroupedBudget> groupedBudgets = [];

            foreach (var budget in Budgets)
            {
                if (budget.BudgetDate.Month == DateTime.Now.Month && budget.BudgetDate.Year == DateTime.Now.Year)
                {
                    groupedBudgets.Add(new GroupedBudget() { Group = "Current", Budget = budget });
                }
                else if (budget.BudgetDate > DateTime.Now)
                {
                    groupedBudgets.Add(new GroupedBudget() { Group = "Future", Budget = budget });
                }
                else
                {
                    // Don't add a budget from more than 1 year ago
                    if (budget.BudgetDate < DateTime.Now.AddYears(-1))
                        continue;

                    groupedBudgets.Add(new GroupedBudget() { Group = "Old", Budget = budget });
                }
            }

            GroupedBudgets = new(groupedBudgets);
            GroupedBudgets.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            GroupedBudgets.CustomSort = new GroupComparer();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(SelectedGroupedBudget):

                    // Find this budget in the budgets collection and load it
                    if (SelectedGroupedBudget != null)
                    {
                        var index = FindBudgetIndex(SelectedGroupedBudget.Budget.BudgetTitle);
                        if (index != -1)
                        {
                            LoadBudget(index);
                        }
                    }

                    break;
                default:
                    break;
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

            // Add savings items as expenses
            foreach (var item in CurrentBudget.BudgetSavingsCategories)
            {
                ExpenseTotal += item.BudgetedAmount;
            }

            // write the items to the database
            WriteToDatabase();

            // Update the charts
            UpdateCharts();

            // Update the left to budget value
            LeftToBudget = new(0m); // trigger an on property changed event to update theme
            LeftToBudget = IncomeTotal - ExpenseTotal;
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

            // Add Savings categories as expenses
            double savingsTotal = 0.0;
            foreach (var item in CurrentBudget.BudgetSavingsCategories)
            {
                savingsTotal += (double)item.BudgetedAmount.Value;
            }
            expenseTotals.Add("Savings", savingsTotal);

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
                // Make sure item doesn't exist
                if (DoesIncomeItemExist(viewModel.BudgetCategory))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists", "A category called \"" + viewModel.BudgetCategory + "\" already exists", "OK");
                    return;
                }

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
        private async Task AddSavingsCategory()
        {
            if (CurrentBudget == null) return;
            if (!IsEditingEnabled) return;

            var viewModel = new SavingsCategoryDialogViewModel();
            _savingsCategoryDialogService.SetViewModel(viewModel);
            var result = await _savingsCategoryDialogService.ShowDialogAsync(_contentDialogService, "New Savings Category");
            viewModel = _savingsCategoryDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure item doesn't exist
                if (DoesSavingsCategoryExist(viewModel.Category))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists", "A category called \"" + viewModel.Category + "\" already exists", "OK");
                    return;
                }

                // Create a new savings category with the results from the dialog
                BudgetSavingsCategory category = new()
                {
                    CategoryName = viewModel.Category,
                    BudgetedAmount = viewModel.Planned,
                    CurrentBalance = viewModel.CurrentBalance,
                };

                // Add the category to the list of savings categories
                CurrentBudget.BudgetSavingsCategories.Add(category);

                // Recalculate the totals for the budget
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
            var result = await _newExpenseGroupDialogService.ShowDialogAsync(_contentDialogService, "New Expense Group", "Add");
            viewModel = _newExpenseGroupDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure a group with this name doesn't already exist
                if (DoesExpenseGroupExist(viewModel.GroupName))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Group Already Exists", "A group called \"" + viewModel.GroupName + "\" already exists", "OK");
                    return;
                }

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
                // Make sure an item with this name doesn't already exist
                if (DoesExpenseItemExist(viewModel.BudgetCategory))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists", "A category called \"" + viewModel.BudgetCategory + "\" already exists", "OK");
                    return;
                }

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
                // Make sure the category name of the edited item doesn't already exist
                if (DoesIncomeItemExist(viewModel.BudgetCategory) && 
                    CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Category != viewModel.BudgetCategory)
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists", "A category called \"" + viewModel.BudgetCategory + "\" already exists", "OK");
                    return;
                }

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
            var result = await _newExpenseGroupDialogService.ShowDialogAsync(_contentDialogService, "Edit Group Name", "Edit");
            viewModel = _newExpenseGroupDialogService.GetViewModel();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure a group with this name doesn't already exist
                if (DoesExpenseGroupExist(viewModel.GroupName) && parameter.CategoryName != viewModel.GroupName)
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Group Already Exists", "A group called \"" + viewModel.GroupName + "\" already exists", "OK");
                    return;
                }

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
                // Make sure an item with this name doesn't already exist
                if (DoesExpenseItemExist(viewModel.BudgetCategory) && 
                    parameter.SubItems[parameter.SelectedSubItemIndex].Category != viewModel.BudgetCategory)
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists", "A category called \"" + viewModel.BudgetCategory + "\" already exists", "OK");
                    return;
                }

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

            // Update the budget totals
            UpdateListViewTotals();
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
                LoadBudget(FindBudgetIndex(budgetTitle));
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
            
            // Select the item in the listview
            foreach (var item in GroupedBudgets)
            {
                if (item is GroupedBudget budget && budget.Budget == CurrentBudget)
                {
                    SelectedGroupedBudgetIndex = GroupedBudgets.IndexOf(item);
                }
            }

            IsEditingEnabled = true;

            AddActualSpentToCurrentBudget();

            UpdateCharts();
            UpdateListViewTotals();
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

        private bool DoesIncomeItemExist(string item)
        {
            if (CurrentBudget == null) return false;

            foreach (var incomeCategory in CurrentBudget.BudgetIncomeItems)
            {
                if (incomeCategory.Category == item)
                    return true;
            }
            return false;
        }

        private bool DoesSavingsCategoryExist(string category)
        {
            if (CurrentBudget == null) return false;
            foreach (var savingsCategory in CurrentBudget.BudgetSavingsCategories)
            {
                if (savingsCategory.CategoryName == category)
                    return true;
            }
            return false;
        }

        private bool DoesExpenseGroupExist(string groupName)
        {
            if (CurrentBudget == null) return false;
            if (groupName == "Savings" || groupName == "Income") return true;

            foreach (var expenseGroup in CurrentBudget.BudgetExpenseItems)
            {
                if (expenseGroup.CategoryName == groupName)
                    return true;
            }
            return false;
        }

        private bool DoesExpenseItemExist(string item)
        {
            if (CurrentBudget == null) return false;

            foreach (var expenseGroup in CurrentBudget.BudgetExpenseItems)
            {
                foreach (var expenseCategory in expenseGroup.SubItems)
                {
                    if (expenseCategory.Category == item)
                        return true;
                }
            }
            return false;
        }

        private void AddActualSpentToCurrentBudget()
        {
            if (CurrentBudget == null) return;

            // get a budget report for the month of the current budget
            var incomeItems = BudgetReportCalculator.CalculateIncomeReportItems(CurrentBudget.BudgetDate, _databaseReader);
            var expenseItems = BudgetReportCalculator.CalculateExpenseReportItems(CurrentBudget.BudgetDate, _databaseReader);

            // go through the report items and set the equivalent budget items' actual amount
            for (int i = 0; i < incomeItems.Count; i++)
            {
                CurrentBudget.BudgetIncomeItems[i].Actual = incomeItems[i].Actual;
            }

            foreach (var expenseGroup in CurrentBudget.BudgetExpenseItems)
            {
                foreach (var subItem in expenseGroup.SubItems)
                {
                    var matchingItem = expenseItems.FirstOrDefault(item => item.Category == subItem.Category);
                    if (matchingItem != null)
                    {
                        subItem.Actual = matchingItem.Actual;
                    }
                }
            }

        }
    }
}
