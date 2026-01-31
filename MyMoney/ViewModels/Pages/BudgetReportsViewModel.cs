using System.Collections.ObjectModel;
using System.Globalization;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;
using SkiaSharp;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MyMoney.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for the budget reports page, displaying detailed budget analysis and charts
    /// </summary>
    public partial class BudgetReportsViewModel : ObservableObject, INavigationAware
    {
        #region Report Data

        /// <summary>
        /// Title of the current report
        /// </summary>
        [ObservableProperty]
        private string _reportTitle = "Budget Report";

        /// <summary>
        /// Collection of income items in the report
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _incomeItems = [];

        /// <summary>
        /// Collection of expense items in the report
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _expenseItems = [];

        /// <summary>
        /// Collection of savings items in the report
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<SavingsCategoryReportItem> _savingsItems = [];

        /// <summary>
        /// Total amount for the current report
        /// </summary>
        [ObservableProperty]
        private Currency _reportTotal = new(0m);

        #endregion

        #region Budget Selection

        /// <summary>
        /// Available budgets for report generation
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Budget> _budgets = [];

        /// <summary>
        /// Currently selected budget
        /// </summary>
        [ObservableProperty]
        private Budget? _selectedBudget;

        /// <summary>
        /// Index of the selected budget in the budgets collection
        /// </summary>
        [ObservableProperty]
        private int _selectedBudgetIndex;

        #endregion

        #region Chart Properties

        /// <summary>
        /// Series data for the income chart
        /// </summary>
        [ObservableProperty]
        private ISeries[] _actualIncomeSeries = [];

        /// <summary>
        /// Title for the income chart
        /// </summary>
        [ObservableProperty]
        private LabelVisual _actualIncomeTitle = new()
        {
            Text = "Actual Income",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
        };

        /// <summary>
        /// Series data for the expense chart
        /// </summary>
        [ObservableProperty]
        private ISeries[] _actualExpenseSeries = [];

        /// <summary>
        /// Title for the expense chart
        /// </summary>
        [ObservableProperty]
        private LabelVisual _actualExpensesTitle = new()
        {
            Text = "Actual Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
        };

        /// <summary>
        /// Current text color for charts, changes with theme
        /// </summary>
        [ObservableProperty]
        private SKColor _chartTextColor = DefaultTextColor;

        #endregion

        #region Constants

        private static readonly SKColor DefaultTextColor = new(0x33, 0x33, 0x33);
        private static readonly SKColor LightTextColor = new(0x33, 0x33, 0x33);
        private static readonly SKColor DarkTextColor = new(0xff, 0xff, 0xff);

        #endregion

        #region Fields

        private readonly IDatabaseManager _databaseReader;

        #endregion

        #region Constructor

        public BudgetReportsViewModel(IDatabaseManager databaseReader)
        {
            _databaseReader = databaseReader ?? throw new ArgumentNullException(nameof(databaseReader));
        }

        #endregion

        #region Public Methods

        public async Task OnNavigatedToAsync()
        {
            await LoadBudgets();
            UpdateCharts();
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        /// <summary>
        /// Calculates the budget report for a specific date
        /// </summary>
        public async Task CalculateReport(DateTime date)
        {
            ClearReports();
            var reportData = await BudgetReportCalculator.CalculateBudgetReport(date, _databaseReader);

            var (incomeTotal, expenseTotal, reportTotal) = await CalculateReportTotals(
                reportData.income,
                reportData.expenses
            );
            reportData.income.Add(incomeTotal);
            reportData.expenses.Add(expenseTotal);
            UpdateReportCollections(reportData);

            ReportTotal = reportTotal;
        }

        #endregion

        #region Event Handlers

        async partial void OnSelectedBudgetChanged(Budget? value)
        {
            if (value == null)
            {
                ReportTitle = "Budget Report";
                return;
            }

            await CalculateReport(value.BudgetDate);
            ReportTitle = value.BudgetTitle;
            UpdateCharts();
        }

        #endregion

        #region Private Methods

        private async Task LoadBudgets()
        {
            BudgetCollection budgetCollection = new(_databaseReader);

            var unsortedBudgets = new ObservableCollection<Budget>(budgetCollection.Budgets);
            Budgets = new ObservableCollection<Budget>(unsortedBudgets.OrderByDescending(o => o.BudgetDate));

            // Select the current budget
            bool found = false;
            foreach (var budget in Budgets)
            {
                if (budget.BudgetTitle == BudgetCollection.GetCurrentBudgetName())
                {
                    int index = Budgets.IndexOf(budget);
                    if (index != SelectedBudgetIndex)
                    {
                        SelectedBudgetIndex = index;
                    }
                    else
                    {
                        // Current budget is already loaded, refresh it's report
                        await CalculateReport(budget.BudgetDate);
                        ReportTitle = budget.BudgetTitle;
                        UpdateCharts();
                    }
                    found = true;
                    break;
                }
            }

            if (!found && Budgets.Count > 0) // No current budget, just select the latest
            {
                if (SelectedBudgetIndex != 0)
                    SelectedBudgetIndex = 0;
                else
                {
                    // Latest budget is already loaded, refresh it
                    await CalculateReport(Budgets[SelectedBudgetIndex].BudgetDate);
                    ReportTitle = Budgets[SelectedBudgetIndex].BudgetTitle;
                    UpdateCharts();
                }
            }
        }

        private void ClearReports()
        {
            IncomeItems.Clear();
            ExpenseItems.Clear();
            SavingsItems.Clear();
        }

        private void UpdateReportCollections(
            (
                List<BudgetReportItem> income,
                List<BudgetReportItem> expense,
                List<SavingsCategoryReportItem> savings
            ) data
        )
        {
            foreach (var item in data.income)
                IncomeItems.Add(item);

            foreach (var item in data.expense)
                ExpenseItems.Add(item);

            foreach (var item in data.savings)
                SavingsItems.Add(item);
        }

        private static async Task<(
            BudgetReportItem income,
            BudgetReportItem expenses,
            Currency total
        )> CalculateReportTotals(IList<BudgetReportItem> incomeItems, IList<BudgetReportItem> expenseItems)
        {
            var incomeTotal = await Task.Run(() => CalculateTotal(incomeItems));
            var expenseTotal = await Task.Run(() => CalculateTotal(expenseItems));

            incomeTotal.Category = "Total";
            expenseTotal.Category = "Total";

            Currency reportTotal = incomeTotal.Actual - expenseTotal.Actual;

            return (incomeTotal, expenseTotal, reportTotal);
        }

        private static BudgetReportItem CalculateTotal(IEnumerable<BudgetReportItem> items)
        {
            var total = new BudgetReportItem();
            foreach (var item in items)
            {
                total.Actual += item.Actual;
                total.Budgeted += item.Budgeted;
                total.Remaining += item.Remaining;
            }
            return total;
        }

        #endregion

        #region Chart Management

        private void UpdateCharts()
        {
            if (SelectedBudget == null)
                return;

            UpdateActualIncomeChart();
            UpdateActualExpensesChart();
            UpdateChartsTheme();
        }

        private void UpdateActualIncomeChart()
        {
            if (SelectedBudget == null)
                return;

            var incomeTotals = GetNonZeroTotals(
                IncomeItems.Take(IncomeItems.Count - 1),
                item => item.Actual.Value,
                item => item.Category
            );

            ActualIncomeSeries = CreatePieSeries(incomeTotals);
        }

        private void UpdateActualExpensesChart()
        {
            if (SelectedBudget == null)
                return;

            var expenseTotals = GetNonZeroTotals(
                ExpenseItems.Take(ExpenseItems.Count - 1),
                item => item.Actual.Value,
                item => item.Category
            );

            var savingsTotals = GetNonZeroTotals(SavingsItems, item => item.Saved.Value, item => item.Category);

            var combinedTotals = expenseTotals.Concat(savingsTotals).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            ActualExpenseSeries = CreatePieSeries(combinedTotals);
        }

        private static Dictionary<string, double> GetNonZeroTotals<T>(
            IEnumerable<T> items,
            Func<T, decimal> valueSelector,
            Func<T, string> categorySelector
        )
        {
            return items
                .Where(item => valueSelector(item) != 0m)
                .ToDictionary(item => categorySelector(item), item => (double)valueSelector(item));
        }

        private static ISeries[] CreatePieSeries(Dictionary<string, double> totals)
        {
            return totals
                .Select(item => new PieSeries<double>
                {
                    Values = [item.Value],
                    Name = item.Key,
                    DataLabelsFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                    ToolTipLabelFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                })
                .ToArray();
        }

        private void UpdateChartsTheme()
        {
            ChartTextColor =
                ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light ? LightTextColor : DarkTextColor;

            var textPaint = new SolidColorPaint(ChartTextColor);
            ActualIncomeTitle.Paint = textPaint;
            ActualExpensesTitle.Paint = textPaint;
        }

        #endregion
    }
}
