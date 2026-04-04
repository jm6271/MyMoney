using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;
using MyMoney.Helpers;
using MyMoney.Helpers.DropHandlers;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using MyMoney.Views.Pages;
using SkiaSharp;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MyMoney.ViewModels.Pages
{
    public partial class BudgetViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private DateTime _selectedBudgetMonth = DateTime.Today;

        [ObservableProperty]
        private Budget? _currentBudget;

        [ObservableProperty]
        private string _newBudgetCopyInfo = string.Empty;

        [ObservableProperty]
        private ISeries[] _incomePercentagesSeries = [];

        [ObservableProperty]
        private ISeries[] _expensePercentagesSeries = [];

        [ObservableProperty]
        private LabelVisual _incomePercentagesTitle = new()
        {
            Text = "Income",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
        };

        [ObservableProperty]
        private LabelVisual _expensePercentagesTitle = new()
        {
            Text = "Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
        };

        // Colors for chart text (changes in light and dark modes)
        [ObservableProperty]
        private SKColor _chartTextColor = new(0x33, 0x33, 0x33);

        [ObservableProperty]
        private int _incomeItemsSelectedIndex = -1;

        [ObservableProperty]
        private int _savingsCategoriesSelectedIndex = -1;

        [ObservableProperty]
        private int _expenseItemsSelectedIndex = -1;

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

        // Drop handlers for drag/drop operations
        public BudgetExpenseGroupReorderHandler ExpenseGroupsReorderHandler { get; }
        public BudgetSavingsCategoryReorderHandler SavingsCategoryReorderHandler { get; }
        public BudgetIncomeItemReorderHandler IncomeItemsReorderHandler { get; }
        public BudgetExpenseItemMoveAndReorderHandler ExpenseItemsMoveAndReorderHandler { get; }

        // Content dialog service
        private readonly IContentDialogService _contentDialogService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IContentDialogFactory _contentDialogFactory;
        private readonly IDatabaseManager _databaseManager;

        public BudgetViewModel(
            IContentDialogService contentDialogService,
            IDatabaseManager databaseManager,
            IMessageBoxService messageBoxService,
            IContentDialogFactory contentDialogFactory
        )
        {
            _contentDialogService = contentDialogService;
            _messageBoxService = messageBoxService;
            _contentDialogFactory = contentDialogFactory;
            _databaseManager = databaseManager;

            // Set up drop handlers
            ExpenseGroupsReorderHandler = new(this);
            SavingsCategoryReorderHandler = new(this);
            IncomeItemsReorderHandler = new(this);
            ExpenseItemsMoveAndReorderHandler = new(this);
        }

        public async Task OnNavigatedToAsync()
        {
            if (CurrentBudget == null)
            {
                await LoadMostRecentBudget();
                if (CurrentBudget == null)
                    await LoadBudget();
            }

            UpdateBudgetTotals(false);

            // Fire property changed so that colors update if the theme was changed
            OnPropertyChanged(nameof(LeftToBudget));
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        public void WriteToDatabase()
        {
            if (CurrentBudget == null)
                return;
            _databaseManager.Update("Budgets", CurrentBudget);
        }

        private void UpdateBudgetTotals(bool writeChanges = true)
        {
            if (CurrentBudget == null)
            {
                ResetTotals();
                return;
            }

            UpdateIncomeTotals();
            UpdateExpenseTotals();

            if (writeChanges)
                WriteToDatabase();

            UpdateCharts();
            UpdateLeftToBudget();
        }

        private void ResetTotals()
        {
            IncomeTotal = new(0m);
            ExpenseTotal = new(0m);
            LeftToBudget = new(0m);
        }

        private void UpdateIncomeTotals()
        {
            if (CurrentBudget == null)
                return;

            IncomeTotal = new(CurrentBudget.BudgetIncomeItems.Sum(item => item.Amount.Value));
        }

        private void UpdateExpenseTotals()
        {
            if (CurrentBudget == null)
                return;

            var expenseSum = CurrentBudget.BudgetExpenseItems.Sum(item => item.CategoryTotal.Value);
            var savingsSum = CurrentBudget.BudgetSavingsCategories.Sum(item => item.BudgetedAmount.Value);

            ExpenseTotal = new(expenseSum + savingsSum);
        }

        private void UpdateLeftToBudget()
        {
            LeftToBudget = new(0m); // Trigger property changed
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
                IncomePercentagesSeries[i] = new PieSeries<double>
                {
                    Values = [item.Value],
                    Name = item.Key,
                    MaxRadialColumnWidth = 20,
                    DataLabelsFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                    ToolTipLabelFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                };

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
                ExpensePercentagesSeries[i] = new PieSeries<double>
                {
                    Values = [item.Value],
                    Name = item.Key,
                    MaxRadialColumnWidth = 20,
                    DataLabelsFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                    ToolTipLabelFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                };
                i++;
            }

            // Update theme
            UpdateChartTheme();
        }

        private void UpdateChartTheme()
        {
            ChartTextColor =
                ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light
                    ? new SKColor(0x33, 0x33, 0x33)
                    : new SKColor(0xff, 0xff, 0xff);

            IncomePercentagesTitle.Paint = new SolidColorPaint(ChartTextColor);
            ExpensePercentagesTitle.Paint = new SolidColorPaint(ChartTextColor);
        }

        [RelayCommand]
        private async Task BudgetMonthChanged()
        {
            await LoadBudget();
        }


        [RelayCommand]
        private async Task AddIncomeItem()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;

            var viewModel = new BudgetCategoryDialogViewModel();

            var dialog = _contentDialogFactory.Create<BudgetCategoryDialog>();
            dialog.Title = "New Income Item";
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure item doesn't exist
                if (DoesIncomeItemExist(viewModel.BudgetCategory))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists",
                        "A category called \"" + viewModel.BudgetCategory + "\" already exists",
                        "OK"
                    );
                    return;
                }

                // Create a new income item with the results from the dialog
                BudgetItem item = new() { Category = viewModel.BudgetCategory, Amount = viewModel.BudgetAmount };

                // Add the item to the budget income items list
                CurrentBudget.BudgetIncomeItems.Add(item);

                // Recalculate the total of the income items
                UpdateBudgetTotals();
            }
        }

        [RelayCommand]
        private async Task AddSavingsCategory()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;

            var viewModel = new SavingsCategoryDialogViewModel()
            {
                RecentTransactionsVisibility = Visibility.Collapsed,
            };

            var dialog = _contentDialogFactory.Create<SavingsCategoryDialog>();
            dialog.DataContext = viewModel;
            dialog.Title = "New Savings Category";
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure item doesn't exist
                if (DoesSavingsCategoryExist(viewModel.Category))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists",
                        "A category called \"" + viewModel.Category + "\" already exists",
                        "OK"
                    );
                    return;
                }

                // Create a new savings category with the results from the dialog
                BudgetSavingsCategory category = new()
                {
                    CategoryName = viewModel.Category,
                    BudgetedAmount = viewModel.Planned,
                    CurrentBalance = viewModel.CurrentBalance + viewModel.Planned,
                };

                // Add a transaction that applies the budgeted amount to the savings category
                Transaction appliedBudgetedAmount = new(
                    CurrentBudget.BudgetDate,
                    "",
                    new Category() { Group = "Savings", Name = category.CategoryName },
                    category.BudgetedAmount,
                    "Planned this month"
                );
                appliedBudgetedAmount.TransactionDetail = "Planned this month";

                category.Transactions.Add(appliedBudgetedAmount);
                category.PlannedTransactionHash = appliedBudgetedAmount.TransactionHash;

                // Add the category to the list of savings categories
                CurrentBudget.BudgetSavingsCategories.Add(category);

                // Recalculate the totals for the budget
                UpdateBudgetTotals();
            }
        }

        [RelayCommand]
        private async Task AddExpenseGroup()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;

            var viewModel = new NewExpenseGroupDialogViewModel();

            var dialog = _contentDialogFactory.Create<NewExpenseGroupDialog>();
            dialog.Title = "New Expense Group";
            dialog.PrimaryButtonText = "Add";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure a group with this name doesn't already exist
                if (DoesExpenseGroupExist(viewModel.GroupName))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Group Already Exists",
                        "A group called \"" + viewModel.GroupName + "\" already exists",
                        "OK"
                    );
                    return;
                }

                // Add a new expense group
                BudgetExpenseCategory expenseGroup = new();
                expenseGroup.CategoryName = viewModel.GroupName;
                CurrentBudget.BudgetExpenseItems.Add(expenseGroup);

                // Update totals and write to database
                UpdateBudgetTotals();
            }
        }

        [RelayCommand]
        private async Task AddExpenseItem(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;

            var viewModel = new BudgetCategoryDialogViewModel();

            var dialog = _contentDialogFactory.Create<BudgetCategoryDialog>();
            dialog.Title = "New Expense Item";
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure an item with this name doesn't already exist
                if (DoesExpenseItemExist(viewModel.BudgetCategory))
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists",
                        "A category called \"" + viewModel.BudgetCategory + "\" already exists",
                        "OK"
                    );
                    return;
                }

                // Create a new expense item with the results from the dialog
                BudgetItem item = new() { Category = viewModel.BudgetCategory, Amount = viewModel.BudgetAmount };

                // Add the item to the budget expense items list
                parameter.SubItems.Add(item);

                // Recalculate the total of the expense items
                UpdateBudgetTotals();
            }
        }

        [RelayCommand]
        private async Task EditIncomeItem()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;
            if (IncomeItemsSelectedIndex < 0)
                return;

            var viewModel = new BudgetCategoryDialogViewModel
            {
                BudgetCategory = CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Category,
                BudgetAmount = CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Amount,
            };

            var dialog = _contentDialogFactory.Create<BudgetCategoryDialog>();
            dialog.Title = "Edit Income Item";
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure the category name of the edited item doesn't already exist
                if (
                    DoesIncomeItemExist(viewModel.BudgetCategory)
                    && CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex].Category != viewModel.BudgetCategory
                )
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists",
                        "A category called \"" + viewModel.BudgetCategory + "\" already exists",
                        "OK"
                    );
                    return;
                }

                // modify the item at the selected index
                BudgetItem incomeItem = new() { Category = viewModel.BudgetCategory, Amount = viewModel.BudgetAmount };

                // assign the selected index of the list with the new item
                CurrentBudget.BudgetIncomeItems[IncomeItemsSelectedIndex] = incomeItem;

                // Recalulate the spent properties of all the items in the budget
                await AddActualSpentToCurrentBudget();

                // Recalculate the total of the income items
                UpdateBudgetTotals();
            }
        }

        [RelayCommand]
        private async Task DeleteIncomeItem()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;
            if (IncomeItemsSelectedIndex < 0)
                return;

            // Show message box asking user if they really want to delete the category
            var result = await _messageBoxService.ShowAsync(
                "Delete Category?",
                "Are you sure you want to delete the selected category?",
                "Yes",
                "No"
            );

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary)
                return; // User clicked no

            CurrentBudget.BudgetIncomeItems.RemoveAt(IncomeItemsSelectedIndex);

            // replace the id property of the remaining elements so the IDs are in a consecutive order (We have all kinds of problems when we don't do this)
            for (var i = 0; i < CurrentBudget.BudgetIncomeItems.Count; i++)
            {
                CurrentBudget.BudgetIncomeItems[i].Id = i + 1;
            }

            UpdateBudgetTotals();
        }

        [RelayCommand]
        private async Task EditSavingsCategory()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;
            if (SavingsCategoriesSelectedIndex < 0)
                return;

            var viewModel = new SavingsCategoryDialogViewModel
            {
                Category = CurrentBudget.BudgetSavingsCategories[SavingsCategoriesSelectedIndex].CategoryName,
                Planned = CurrentBudget.BudgetSavingsCategories[SavingsCategoriesSelectedIndex].BudgetedAmount,
                CurrentBalance = CurrentBudget.BudgetSavingsCategories[SavingsCategoriesSelectedIndex].CurrentBalance,
                RecentTransactions = CurrentBudget.BudgetSavingsCategories[SavingsCategoriesSelectedIndex].Transactions,
            };
            viewModel.SortTransactions();

            var dialog = _contentDialogFactory.Create<SavingsCategoryDialog>();
            dialog.DataContext = viewModel;
            dialog.Title = "Edit Savings Category";
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure the category name of the edited item doesn't already exist
                if (
                    DoesSavingsCategoryExist(viewModel.Category)
                    && CurrentBudget.BudgetSavingsCategories[SavingsCategoriesSelectedIndex].CategoryName
                        != viewModel.Category
                )
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists",
                        "A category called \"" + viewModel.Category + "\" already exists",
                        "OK"
                    );
                    return;
                }

                // modify the item at the selected index
                BudgetSavingsCategory originalSavingsCategory = (BudgetSavingsCategory)
                    CurrentBudget.BudgetSavingsCategories[SavingsCategoriesSelectedIndex].Clone();
                BudgetSavingsCategory editedSavingsCategory = CurrentBudget.BudgetSavingsCategories[
                    SavingsCategoriesSelectedIndex
                ];
                editedSavingsCategory.CategoryName = viewModel.Category;

                // Adjust the balance if it has changed
                if (editedSavingsCategory.CurrentBalance != viewModel.CurrentBalance)
                {
                    // Add a transaction to show that the balance was updated
                    Transaction updatedTransaction = new(
                        DateTime.Today,
                        "",
                        new Category() { Group = "Savings", Name = editedSavingsCategory.CategoryName },
                        new(viewModel.CurrentBalance.Value - editedSavingsCategory.CurrentBalance.Value),
                        "Updated balance"
                    );
                    updatedTransaction.TransactionDetail = "Updated balance";
                    editedSavingsCategory.Transactions.Add(updatedTransaction);

                    // Update the balance
                    editedSavingsCategory.CurrentBalance = viewModel.CurrentBalance;
                }

                // Update the planned amount if it has changed
                if (editedSavingsCategory.BudgetedAmount != viewModel.Planned)
                {
                    for (int i = 0; i < editedSavingsCategory.Transactions.Count; i++)
                    {
                        if (
                            editedSavingsCategory.Transactions[i].TransactionHash
                            == editedSavingsCategory.PlannedTransactionHash
                        )
                        {
                            // Remove the amount of this transaction from the balance
                            editedSavingsCategory.CurrentBalance -= editedSavingsCategory.BudgetedAmount;

                            // Apply the new amount
                            editedSavingsCategory.CurrentBalance += viewModel.Planned;
                            editedSavingsCategory.BudgetedAmount = viewModel.Planned;

                            // Modify the transaction
                            editedSavingsCategory.Transactions[i].Amount = editedSavingsCategory.BudgetedAmount;
                        }
                    }
                }

                // assign the selected index of the list with the new item
                CurrentBudget.BudgetSavingsCategories[SavingsCategoriesSelectedIndex] = editedSavingsCategory;

                // Update the category in any future budgets
                await UpdateFutureSavingsCategories(originalSavingsCategory, editedSavingsCategory);

                // Recalulate the spent properties of all the items in the budget
                await AddActualSpentToCurrentBudget();

                // Recalculate the total of the income items
                UpdateBudgetTotals();
            }
        }

        [RelayCommand]
        private async Task DeleteSavingsCategory()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;
            if (SavingsCategoriesSelectedIndex < 0)
                return;

            // Show message box asking user if they really want to delete the category
            var result = await _messageBoxService.ShowAsync(
                "Delete Category?",
                "Are you sure you want to delete the selected category?",
                "Yes",
                "No"
            );

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary)
                return; // User clicked no

            CurrentBudget.BudgetSavingsCategories.RemoveAt(SavingsCategoriesSelectedIndex);

            // replace the id property of the remaining elements so the IDs are in a consecutive order (We have all kinds of problems when we don't do this)
            for (var i = 0; i < CurrentBudget.BudgetSavingsCategories.Count; i++)
            {
                CurrentBudget.BudgetSavingsCategories[i].Id = i + 1;
            }

            UpdateBudgetTotals();
        }

        [RelayCommand]
        private async Task EditExpenseGroup(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;

            var viewModel = new NewExpenseGroupDialogViewModel { GroupName = parameter.CategoryName };

            var dialog = _contentDialogFactory.Create<NewExpenseGroupDialog>();
            dialog.Title = "Edit Group Name";
            dialog.PrimaryButtonText = "Edit";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure a group with this name doesn't already exist
                if (DoesExpenseGroupExist(viewModel.GroupName) && parameter.CategoryName != viewModel.GroupName)
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Group Already Exists",
                        "A group called \"" + viewModel.GroupName + "\" already exists",
                        "OK"
                    );
                    return;
                }

                parameter.CategoryName = viewModel.GroupName;
            }
        }

        [RelayCommand]
        private async Task EditExpenseItem(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;
            if (parameter.SelectedSubItemIndex < 0 || parameter.SelectedSubItemIndex > parameter.SubItems.Count - 1)
                return;

            var viewModel = new BudgetCategoryDialogViewModel
            {
                BudgetCategory = parameter.SubItems[parameter.SelectedSubItemIndex].Category,
                BudgetAmount = parameter.SubItems[parameter.SelectedSubItemIndex].Amount,
            };

            var dialog = _contentDialogFactory.Create<BudgetCategoryDialog>();
            dialog.Title = "Edit Expense Item";
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                // Make sure an item with this name doesn't already exist
                if (
                    DoesExpenseItemExist(viewModel.BudgetCategory)
                    && parameter.SubItems[parameter.SelectedSubItemIndex].Category != viewModel.BudgetCategory
                )
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Category Already Exists",
                        "A category called \"" + viewModel.BudgetCategory + "\" already exists",
                        "OK"
                    );
                    return;
                }

                // modify the item at the selected index
                BudgetItem expenseItem = new() { Category = viewModel.BudgetCategory, Amount = viewModel.BudgetAmount };

                // assign the selected index of the list with the new item
                parameter.SubItems[parameter.SelectedSubItemIndex] = expenseItem;

                // Recalute the spent properties of all the items in the budget
                await AddActualSpentToCurrentBudget();

                // Recalculate the total of the expense items
                UpdateBudgetTotals();
            }
        }

        [RelayCommand]
        private async Task DeleteExpenseGroup(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;

            // as user if they really want to delete the group
            var result = await _messageBoxService.ShowAsync(
                "Delete Group?",
                "Are you sure you want to delete this group?",
                "Yes",
                "No"
            );

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary)
                return; // User clicked no

            // delete the item
            CurrentBudget.BudgetExpenseItems.Remove(parameter);

            // Update the budget totals
            UpdateBudgetTotals();
        }

        [RelayCommand]
        private async Task DeleteExpenseItem(BudgetExpenseCategory parameter)
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;
            if (parameter.SelectedSubItemIndex < 0 || parameter.SelectedSubItemIndex > parameter.SubItems.Count - 1)
                return;

            // Show message box asking user if they really want to delete the category
            var result = await _messageBoxService.ShowAsync(
                "Delete Category?",
                "Are you sure you want to delete the selected category?",
                "Yes",
                "No"
            );

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary)
                return; // User clicked no

            parameter.SubItems.RemoveAt(parameter.SelectedSubItemIndex);

            // replace the id property of the remaining elements so the IDs are
            // in a consecutive order (We have all kinds of problems when we don't do this)
            for (int i = 0; i < parameter.SubItems.Count; i++)
            {
                parameter.SubItems[i].Id = i + 1;
            }

            UpdateBudgetTotals();
        }

        [RelayCommand]
        private async Task CreateNewBudget()
        {
            Budget newBudget = new();

            var budgetTitle = SelectedBudgetMonth.ToString("MMMM, yyyy");
            newBudget.BudgetTitle = budgetTitle;
            newBudget.BudgetDate = SelectedBudgetMonth;

            // See if there's a budget to copy categories from
            DateTime? mostRecentDate = null;
            await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
            {
                await Task.Run(() =>
                {
                    mostRecentDate = query.Where(p => p.BudgetDate < SelectedBudgetMonth)
                                          .OrderByDescending(p => p.BudgetDate)
                                          .FirstOrDefault()
                                          ?.BudgetDate;
                });
            });

            // If there is a budget to copy from, select it
            if (mostRecentDate != null)
            {
                SelectedBudgetMonth = mostRecentDate.Value;
                await LoadBudget();
                if (CurrentBudget != null)
                {

                    foreach (var item in CurrentBudget.BudgetIncomeItems)
                    {
                        newBudget.BudgetIncomeItems.Add((BudgetItem)item.Clone());
                    }

                    foreach (var item in CurrentBudget.BudgetSavingsCategories)
                    {
                        // We have to modify the item before copying it over
                        BudgetSavingsCategory newSavingsCategory = (BudgetSavingsCategory)item.Clone();

                        // Add a new transaction that carries the balance forward
                        Transaction balanceCarriedForward = new(
                            newBudget.BudgetDate.AddDays(-1),
                            "",
                            new Category() { Group = "Savings", Name = item.CategoryName },
                            item.CurrentBalance,
                            "Balance carried forward"
                        )
                        {
                            TransactionDetail = CurrentBudget.BudgetDate.ToString("MMM") + " balance",
                        };
                        newSavingsCategory.Transactions.Add(balanceCarriedForward);

                        // Add a transaction that applies the budgeted amount to the total balance
                        Transaction appliedBudgetedAmount = new(
                            newBudget.BudgetDate,
                            "",
                            new Category() { Group = "Savings", Name = item.CategoryName },
                            item.BudgetedAmount,
                            "Planned This Month"
                        );
                        appliedBudgetedAmount.TransactionDetail = "Planned This Month";

                        newSavingsCategory.Transactions.Add(appliedBudgetedAmount);
                        newSavingsCategory.PlannedTransactionHash = appliedBudgetedAmount.TransactionHash;
                        newSavingsCategory.BalanceTransactionHash = balanceCarriedForward.TransactionHash;

                        newSavingsCategory.CurrentBalance += newSavingsCategory.BudgetedAmount;

                        newBudget.BudgetSavingsCategories.Add(newSavingsCategory);
                    }

                    foreach (var item in CurrentBudget.BudgetExpenseItems)
                    {
                        newBudget.BudgetExpenseItems.Add((BudgetExpenseCategory)item.Clone());
                    }
                }
            }

            // Insert into database collection
            _databaseManager.Insert("Budgets", newBudget);

            // Set as current budget
            SelectedBudgetMonth = newBudget.BudgetDate;
            await LoadBudget();
        }

        [RelayCommand]
        private async Task DeleteBudget()
        {
            if (CurrentBudget == null)
                return;
            if (!IsEditingEnabled)
                return;
            // Show message box asking user if they really want to delete the budget
            var result = await _messageBoxService.ShowAsync(
                "Delete Budget?",
                "Are you sure you want to delete the selected budget?\n" + "THIS CANNOT BE UNDONE!",
                "Yes",
                "No"
            );
            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary)
                return; // User clicked no

            _databaseManager.Delete<Budget>("Budgets", CurrentBudget.Id);

            // Load another budget
            await LoadMostRecentBudget();
        }

        [RelayCommand]
        private async Task NextBudgetMonth()
        {
            SelectedBudgetMonth = SelectedBudgetMonth.AddMonths(1);
            await LoadBudget();
        }

        [RelayCommand]
        private async Task PreviousBudgetMonth()
        {
            SelectedBudgetMonth = SelectedBudgetMonth.AddMonths(-1);
            await LoadBudget();
        }

        private async Task<bool> DoesBudgetExist(DateTime selectedDate)
        {
            // Load budget for selected month if it exists
            Budget? budget = null;
            await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
            {
                await Task.Run(() =>
                {
                    budget = query.Where(p => p.BudgetDate.Month == selectedDate.Month && p.BudgetDate.Year == selectedDate.Year)
                                  .FirstOrDefault();
                });
            });

            if (budget == null)
                return false;
            return true;
        }

        private async Task LoadBudget()
        {
            // If budget for requested date is already loaded, then skip loading it again
            if (CurrentBudget?.BudgetDate.Month == SelectedBudgetMonth.Month && CurrentBudget?.BudgetDate.Year == SelectedBudgetMonth.Year)
                return;

            // Load budget for selected month if it exists
            Budget? budget = null;
            await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
            {
                await Task.Run(() =>
                {
                    budget = query.Where(p => p.BudgetDate.Month == SelectedBudgetMonth.Month && p.BudgetDate.Year == SelectedBudgetMonth.Year)
                                  .FirstOrDefault();
                });
            });

            if (budget != null)
            {
                CurrentBudget = budget;
                IsEditingEnabled = true;
            }
            else
            {
                CurrentBudget = null;
                IsEditingEnabled = false;

                // Show info for creating a new budget
                // Look for a previous budget to copy from
                DateTime? mostRecentDate = null;
                await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
                {
                    await Task.Run(() =>
                    {
                        mostRecentDate = query.Where(p => p.BudgetDate < SelectedBudgetMonth)
                                              .OrderByDescending(p => p.BudgetDate)
                                              .FirstOrDefault()
                                              ?.BudgetDate;
                    });
                });

                if (mostRecentDate != null)
                {
                    NewBudgetCopyInfo = $"We'll copy {mostRecentDate.Value:MMMM}'s budget to help you get started.";
                }
                else
                {
                    NewBudgetCopyInfo = ""; // No budget to copy from, we'll just create a blank budget
                }
            }

            UpdateCharts();
            UpdateBudgetTotals();

            await AddActualSpentToCurrentBudget();
        }

        private async Task LoadMostRecentBudget()
        {
            DateTime budgetDate = DateTime.Today;

            await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
            {
                await Task.Run(() =>
                {
                    budgetDate = query.Where(p => p.BudgetDate <= DateTime.Today)
                    .OrderByDescending(p => p.BudgetDate)
                    .FirstOrDefault()?.BudgetDate ?? DateTime.Today;
                });
            });

            SelectedBudgetMonth = budgetDate;
            await LoadBudget();
        }

        private bool DoesIncomeItemExist(string item)
        {
            if (CurrentBudget == null)
                return false;

            foreach (var incomeCategory in CurrentBudget.BudgetIncomeItems)
            {
                if (incomeCategory.Category == item)
                    return true;
            }
            return false;
        }

        private bool DoesSavingsCategoryExist(string category)
        {
            if (CurrentBudget == null)
                return false;
            foreach (var savingsCategory in CurrentBudget.BudgetSavingsCategories)
            {
                if (savingsCategory.CategoryName == category)
                    return true;
            }
            return false;
        }

        private bool DoesExpenseGroupExist(string groupName)
        {
            if (CurrentBudget == null)
                return false;
            if (groupName == "Savings" || groupName == "Income")
                return true;

            foreach (var expenseGroup in CurrentBudget.BudgetExpenseItems)
            {
                if (expenseGroup.CategoryName == groupName)
                    return true;
            }
            return false;
        }

        private bool DoesExpenseItemExist(string item)
        {
            if (CurrentBudget == null)
                return false;

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

        private async Task AddActualSpentToCurrentBudget()
        {
            if (CurrentBudget == null)
                return;

            var budget = CurrentBudget;
            var budgetDate = budget.BudgetDate;

            var (income, expenses, savings) = await BudgetReportCalculator.CalculateBudgetReport(
                budgetDate,
                _databaseManager
            );

            if (CurrentBudget == null || CurrentBudget != budget)
                return; // Ensure budget hasn't changed

            UpdateIncomeActuals(income);
            UpdateSavingsActuals(savings);
            UpdateExpenseActuals(expenses);
        }

        private void UpdateIncomeActuals(List<BudgetReportItem> incomeItems)
        {
            if (CurrentBudget == null)
                return;

            for (int i = 0; i < Math.Min(incomeItems.Count, CurrentBudget.BudgetIncomeItems.Count); i++)
            {
                CurrentBudget.BudgetIncomeItems[i].Actual = incomeItems[i].Actual;
            }
        }

        private void UpdateSavingsActuals(List<SavingsCategoryReportItem> savingsItems)
        {
            if (CurrentBudget == null)
                return;

            for (int i = 0; i < Math.Min(savingsItems.Count, CurrentBudget.BudgetSavingsCategories.Count); i++)
            {
                CurrentBudget.BudgetSavingsCategories[i].Spent = savingsItems[i].Spent;
            }
        }

        private void UpdateExpenseActuals(List<BudgetReportItem> expenseItems)
        {
            if (CurrentBudget == null)
                return;

            foreach (var expenseGroup in CurrentBudget.BudgetExpenseItems)
            {
                foreach (var subItem in expenseGroup.SubItems)
                {
                    var matchingItem = expenseItems.FirstOrDefault(item =>
                        item.Category == subItem.Category && item.Group == expenseGroup.CategoryName
                    );

                    if (matchingItem != null)
                    {
                        subItem.Actual = matchingItem.Actual;
                    }
                }
            }
        }

        private async Task UpdateFutureSavingsCategories(
            BudgetSavingsCategory originalSavingsCategory,
            BudgetSavingsCategory modifiedSavingsCategory
        )
        {
            if (CurrentBudget == null)
                return;

            var earliestDate = CurrentBudget.BudgetDate;

            await Task.Run(async () =>
            {
                List<Budget> Budgets = [];

                await _databaseManager.QueryAsync<Budget>("Budgets", async query =>
                {
                    Budgets.AddRange(query.Where(p => p.BudgetDate > earliestDate).ToList());
                });

                // Loop through future budgets, beginning with the nearest in date and going to the most distant
                for (int i = 0; i < Budgets.Count; i++)
                {
                    // Find the savings category in this budget
                    for (int j = 0; j < Budgets[i].BudgetSavingsCategories.Count; j++)
                    {
                        var currentSavingsCategory = Budgets[i].BudgetSavingsCategories[j];

                        if (
                            currentSavingsCategory.CategoryHash == modifiedSavingsCategory.CategoryHash // Same category
                            && currentSavingsCategory.Transactions.Count != 0
                        ) // There must be some transactions
                        {
                            // Amount to update by
                            var balanceDifference =
                                modifiedSavingsCategory.CurrentBalance - originalSavingsCategory.CurrentBalance;

                            // Update begining balance
                            foreach (
                                var transaction in from Transaction transaction in currentSavingsCategory.Transactions
                                                   where transaction.TransactionHash == currentSavingsCategory.BalanceTransactionHash
                                                   select transaction
                            )
                            {
                                transaction.Amount += balanceDifference;
                            }

                            // Update balance
                            currentSavingsCategory.CurrentBalance += balanceDifference;
                        }
                    }

                    // Update budget in database
                    _databaseManager.Update("Budgets", Budgets[i]);
                }
            });
        }
    }
}
