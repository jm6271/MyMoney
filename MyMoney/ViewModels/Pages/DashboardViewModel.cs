using System.Collections.ObjectModel;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;
using MyMoney.Core.Database;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Wpf.Ui.Appearance;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace MyMoney.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for the dashboard page, displaying account summaries and budget reports
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        #region Collections

        /// <summary>
        /// Collection of account summaries to display on the dashboard
        /// </summary>
        public ObservableCollection<AccountDashboardDisplayItem> Accounts { get; } = [];

        /// <summary>
        /// Collection of income items from the budget report
        /// </summary>
        public ObservableCollection<BudgetReportItem> BudgetReportIncomeItems { get; } = [];

        /// <summary>
        /// Collection of expense items from the budget report
        /// </summary>
        public ObservableCollection<BudgetReportItem> BudgetReportExpenseItems { get; } = [];

        /// <summary>
        /// Collection of savings category items from the budget report
        /// </summary>
        public ObservableCollection<SavingsCategoryReportItem> BudgetReportSavingsItems { get; } = [];

        #endregion

        #region Budget Summary Properties

        [ObservableProperty]
        private double _income;

        [ObservableProperty]
        private double _expenses;

        [ObservableProperty]
        private Currency _differenceTotal = new(0m);

        #endregion

        #region Chart Properties

        [ObservableProperty]
        private ISeries[] _series = [];

        [ObservableProperty]
        private SKColor _chartTextColor = new(0x33, 0x33, 0x33);

        [ObservableProperty]
        private LabelVisual _chartTitle = new()
        {
            Text = "Income vs. Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        [ObservableProperty]
        private Axis[] _xAxes =
        [
            new()
            {
                Labels = ["Income", "Expenses"],
                LabelsRotation = 0,
                TicksAtCenter = true,
                ForceStepToMin = true,
                MinStep = 1
            }
        ];

        [ObservableProperty]
        private Axis[] _yAxes =
        [
            new()
            {
                Name = "Amount",
                Labeler = Labelers.Currency,
            }
        ];

        #endregion

        #region Grid Properties

        [ObservableProperty]
        private int _categoryColumnWidth = 200;

        [ObservableProperty]
        private int _budgetedColumnWidth = 100;

        [ObservableProperty]
        private int _actualColumnWidth = 100;

        [ObservableProperty]
        private int _differenceColumnWidth = 100;

        #endregion

        private readonly IDatabaseReader _databaseReader;
        private readonly object _databaseLockObject = new();

        public DashboardViewModel(IDatabaseReader databaseReader)
        {
            _databaseReader = databaseReader ?? throw new ArgumentNullException(nameof(databaseReader));
            Series = UpdateChartSeries();
        }

        private void CalculateBudgetReport()
        {
            ClearReports();
            var reportItems = LoadReportItems();
            UpdateReportCollections(reportItems);
            CalculateTotals();
            UpdateChartDisplay();
        }

        private void ClearReports()
        {
            BudgetReportIncomeItems.Clear();
            BudgetReportExpenseItems.Clear();
            BudgetReportSavingsItems.Clear();
        }

        private (List<BudgetReportItem> income, List<BudgetReportItem> expense, List<SavingsCategoryReportItem> savings) LoadReportItems()
        {
            lock (_databaseLockObject)
            {
                return (
                    BudgetReportCalculator.CalculateIncomeReportItems(_databaseReader),
                    BudgetReportCalculator.CalculateExpenseReportItems(_databaseReader),
                    BudgetReportCalculator.CalculateSavingsReportItems(DateTime.Today, _databaseReader)
                );
            }
        }

        private void UpdateReportCollections((List<BudgetReportItem> income, List<BudgetReportItem> expense, List<SavingsCategoryReportItem> savings) items)
        {
            foreach (var item in items.income)
                BudgetReportIncomeItems.Add(item);

            foreach (var item in items.expense)
                BudgetReportExpenseItems.Add(item);

            foreach (var item in items.savings)
                BudgetReportSavingsItems.Add(item);
        }

        private void CalculateTotals()
        {
            var incomeTotal = CalculateIncomeTotal();
            var expenseTotal = CalculateExpenseTotal();

            BudgetReportIncomeItems.Add(incomeTotal);
            BudgetReportExpenseItems.Add(expenseTotal);

            Income = (double)incomeTotal.Actual.Value;
            Expenses = (double)expenseTotal.Actual.Value;
            DifferenceTotal = incomeTotal.Actual - expenseTotal.Actual;
        }

        private BudgetReportItem CalculateIncomeTotal()
        {
            var total = new BudgetReportItem { Category = "Total" };
            foreach (var item in BudgetReportIncomeItems)
            {
                total.Actual += item.Actual;
                total.Budgeted += item.Budgeted;
                total.Remaining += item.Remaining;
            }
            return total;
        }

        private BudgetReportItem CalculateExpenseTotal()
        {
            var total = new BudgetReportItem { Category = "Total" };
            foreach (var item in BudgetReportExpenseItems)
            {
                total.Actual += item.Actual;
                total.Budgeted += item.Budgeted;
                total.Remaining += item.Remaining;
            }
            return total;
        }

        private void UpdateChartDisplay()
        {
            Series = UpdateChartSeries();
            UpdateChartTheme();
        }

        private ISeries[] UpdateChartSeries()
        {
            var past12MonthsIncome = IncomeExpense12MonthCalculator.GetPast12MonthsIncome();
            var past12MonthsExpenses = IncomeExpense12MonthCalculator.GetPast12MonthsExpenses();

            var incomeTotal = past12MonthsIncome.Count > 0 ? past12MonthsIncome[^1] : 0.0;
            var expenseTotal = past12MonthsExpenses.Count > 0 ? past12MonthsExpenses[^1] : 0.0;

            var accentColor = ApplicationAccentColorManager.GetColorizationColor();
            return [new ColumnSeries<double>
            {
                Values = [incomeTotal, expenseTotal],
                Fill = new SolidColorPaint(new SKColor(accentColor.R, accentColor.G, accentColor.B)),
                Stroke = null,
            }];
        }

        private void UpdateChartTheme()
        {
            ChartTextColor = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light 
                ? new SKColor(0x33, 0x33, 0x33) 
                : new SKColor(0xff, 0xff, 0xff);

            var textPaint = new SolidColorPaint(ChartTextColor);
            XAxes[0].LabelsPaint = textPaint;
            YAxes[0].LabelsPaint = textPaint;
            YAxes[0].NamePaint = textPaint;
            ChartTitle.Paint = textPaint;
        }

        /// <summary>
        /// Loads account information and updates the dashboard when navigating to the page
        /// </summary>
        public void OnPageNavigatedTo()
        {
            LoadAccounts();
            CalculateBudgetReport();
        }

        private void LoadAccounts()
        {
            Accounts.Clear();

            List<Account> accounts;
            lock (_databaseLockObject)
            {
                accounts = _databaseReader.GetCollection<Account>("Accounts");
            }

            foreach (var account in accounts)
            {
                Accounts.Add(AccountDashboardDisplayItem.FromAccount(account));
            }

            AddAccountTotalItem();
        }

        private void AddAccountTotalItem()
        {
            var totalItem = new AccountDashboardDisplayItem
            {
                AccountName = "Total"
            };

            foreach (var account in Accounts)
            {
                totalItem.Total += account.Total;
            }

            Accounts.Add(totalItem);
        }
    }
}
