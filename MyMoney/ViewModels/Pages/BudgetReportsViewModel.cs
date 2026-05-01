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
        /// Month currently selected for viewing reports.
        /// </summary>
        [ObservableProperty]
        private DateTime _selectedReportMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

        /// <summary>
        /// Budget associated with the currently selected report month, if one exists.
        /// </summary>
        [ObservableProperty]
        private Budget? _currentReportBudget;

        #endregion

        #region Chart Properties

        /// <summary>
        /// Series data for the income chart
        /// </summary>
        [ObservableProperty]
        private ISeries[] _actualIncomeSeries = [];

        /// <summary>
        /// Series data for the expense chart
        /// </summary>
        [ObservableProperty]
        private ISeries[] _actualExpenseSeries = [];

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
            var initialMonth = await GetInitialReportMonthAsync();
            var normalizedInitialMonth = new DateTime(initialMonth.Year, initialMonth.Month, 1);

            if (SelectedReportMonth == normalizedInitialMonth)
            {
                await NavigateToReportMonthAsync(normalizedInitialMonth);
                return;
            }

            SelectedReportMonth = normalizedInitialMonth;
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

        [RelayCommand]
        private void ReportMonthChanged(DateTime selectedMonth)
        {
            SelectedReportMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        }

        [RelayCommand]
        private void NextReportMonth()
        {
            SelectedReportMonth = SelectedReportMonth.AddMonths(1);
        }

        [RelayCommand]
        private void PreviousReportMonth()
        {
            SelectedReportMonth = SelectedReportMonth.AddMonths(-1);
        }

        #endregion

        #region Event Handlers

        partial void OnSelectedReportMonthChanged(DateTime value)
        {
            _ = NavigateToReportMonthAsync(value);
        }

        #endregion

        #region Private Methods

        private void ClearReports()
        {
            IncomeItems.Clear();
            ExpenseItems.Clear();
            SavingsItems.Clear();
            ReportTotal = new Currency(0m);
        }

        private async Task NavigateToReportMonthAsync(DateTime month)
        {
            var normalizedMonth = new DateTime(month.Year, month.Month, 1);
            var budgetForMonth = await GetBudgetForMonthAsync(normalizedMonth);

            CurrentReportBudget = budgetForMonth;

            if (budgetForMonth == null)
            {
                ShowBlankReportForMonth(normalizedMonth);
                return;
            }

            await CalculateReport(budgetForMonth.BudgetDate);
            ReportTitle = budgetForMonth.BudgetTitle;
            UpdateCharts();
        }

        private async Task<Budget?> GetBudgetForMonthAsync(DateTime month)
        {
            Budget? budgetForMonth = null;
            await _databaseReader.QueryAsync<Budget>("Budgets", async query =>
            {
                budgetForMonth = query
                    .Where(b => b.BudgetDate.Month == month.Month && b.BudgetDate.Year == month.Year)
                    .FirstOrDefault();
            });

            return budgetForMonth;
        }

        private async Task<Budget?> GetLatestBudgetAsync()
        {
            Budget? latestBudget = null;
            await _databaseReader.QueryAsync<Budget>("Budgets", async query =>
            {
                latestBudget = query.OrderByDescending(b => b.BudgetDate).FirstOrDefault();
            });

            return latestBudget;
        }

        private async Task<DateTime> GetInitialReportMonthAsync()
        {
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var currentBudget = await GetBudgetForMonthAsync(currentMonth);

            if (currentBudget != null)
                return currentMonth;

            var latestBudget = await GetLatestBudgetAsync();
            if (latestBudget != null)
                return new DateTime(latestBudget.BudgetDate.Year, latestBudget.BudgetDate.Month, 1);

            return currentMonth;
        }

        private void ShowBlankReportForMonth(DateTime month)
        {
            ClearReports();
            ReportTitle = month.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
            ActualIncomeSeries = [];
            ActualExpenseSeries = [];
            UpdateChartsTheme();
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
            if (CurrentReportBudget == null)
            {
                ActualIncomeSeries = [];
                ActualExpenseSeries = [];
                UpdateChartsTheme();
                return;
            }

            UpdateActualIncomeChart();
            UpdateActualExpensesChart();
            UpdateChartsTheme();
        }

        private void UpdateActualIncomeChart()
        {
            if (CurrentReportBudget == null)
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
            if (CurrentReportBudget == null)
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
        }

        #endregion
    }
}
