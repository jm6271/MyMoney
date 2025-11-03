using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyMoney.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for the dashboard page, displaying account summaries and budget reports
    /// </summary>
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        #region Collections

        /// <summary>
        /// Collection of account summaries to display on the dashboard
        /// </summary>
        public ObservableCollection<AccountDashboardDisplayItem> Accounts { get; } = [];

        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _budgetReportIncomeItems = [];

        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _budgetReportExpenseItems = [];

        [ObservableProperty]
        private ObservableCollection<SavingsCategoryReportItem> _budgetReportSavingsItems = [];

        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _budgetReportItems = [];

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

        #region Net Worth Card Properties

        [ObservableProperty]
        private Currency _totalNetWorth = new(0m);

        [ObservableProperty]
        private ISeries[] _netWorthSeries = [];

        [ObservableProperty]
        private Axis[] _netWorthXAxes =
        [
            new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM dd"))
            {
                LabelsPaint = null,
            }
        ];

        [ObservableProperty]
        private Axis[] _netWorthYAxes =
        [
            new ()
            {
                LabelsPaint = null,
                ShowSeparatorLines = false,
            }
        ];

        #endregion

        private readonly IDatabaseManager _databaseReader;
        private readonly Lock _incomeItemsLock = new();
        private readonly Lock _expenseItemsLock = new();
        private readonly Lock _savingsItemsLock = new();
        private readonly Lock _reportItemsLock = new();

        public DashboardViewModel(IDatabaseManager databaseReader)
        {
            _databaseReader = databaseReader ?? throw new ArgumentNullException(nameof(databaseReader));
            Series = UpdateChartSeries();
        }

        private async Task CalculateBudgetReport()
        {
            ClearReports();
            var reportItems = await Task.Run(LoadReportItems);
            UpdateReportCollections(reportItems);
            await UpdateChartDisplay();
        }

        private void ClearReports()
        {
            lock (_expenseItemsLock)
                BudgetReportExpenseItems.Clear();
            lock (_incomeItemsLock)
                BudgetReportIncomeItems.Clear();
            lock (_savingsItemsLock)
                BudgetReportSavingsItems.Clear();
            lock (_reportItemsLock)
                BudgetReportItems.Clear();
        }

        private (List<BudgetReportItem> income, List<BudgetReportItem> expense, List<SavingsCategoryReportItem> savings) LoadReportItems()
        {
            var reportItems = BudgetReportCalculator.CalculateBudgetReport(DateTime.Today, _databaseReader);
            var incomeTotal = CalculateTotal(reportItems.income);
            var expenseTotal = CalculateTotal(reportItems.expenses);
            //reportItems.income.Add(incomeTotal);
            //reportItems.expenses.Add(expenseTotal);

            Income = (double)incomeTotal.Actual.Value;
            Expenses = (double)expenseTotal.Actual.Value;
            DifferenceTotal = incomeTotal.Actual - expenseTotal.Actual;


            return reportItems;
        }

        private void UpdateReportCollections((List<BudgetReportItem> income, List<BudgetReportItem> expense, List<SavingsCategoryReportItem> savings) items)
        {
            lock (_incomeItemsLock)
                BudgetReportIncomeItems = new ObservableCollection<BudgetReportItem>(items.income);

            // Load the expense items one at a time instead of replacing the whole collection,
            // because there are some binding issues with the BudgetReportControl's expense listview's ListCollectionView
            lock (_expenseItemsLock)
                foreach (var item in items.expense)
                    BudgetReportExpenseItems.Add(item);

            lock (_savingsItemsLock)
                BudgetReportSavingsItems = new ObservableCollection<SavingsCategoryReportItem>(items.savings);

            // Copy all items to the report collection
            lock (_reportItemsLock)
            {
                foreach (var item in items.income)
                {
                    BudgetReportItems.Add(item);
                }

                foreach (var item in items.savings)
                {
                    BudgetReportItem savingsItem = new()
                    {
                        Group = "Savings",
                        Category = item.Category,
                        Budgeted = item.Saved,
                        Actual = item.Spent,
                        Remaining = item.Balance,
                        IsExpense = false
                    };

                    BudgetReportItems.Add(savingsItem);
                }

                foreach (var item in items.expense)
                {
                    BudgetReportItems.Add(item);
                }
            }
        }

        private static BudgetReportItem CalculateTotal(List<BudgetReportItem> reportItems)
        {
            var total = new BudgetReportItem { Category = "Total" };
            foreach (var item in reportItems)
            {
                total.Actual += item.Actual;
                total.Budgeted += item.Budgeted;
                total.Remaining += item.Remaining;
            }
            return total;
        }

        private async Task UpdateChartDisplay()
        {
            Series = UpdateChartSeries();
            await UpdateNetWorthChart();
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

        public async Task OnNavigatedToAsync()
        {
            LoadAccounts();
            await CalculateBudgetReport();
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        private void LoadAccounts()
        {
            Accounts.Clear();

            List<Account> accounts = _databaseReader.GetCollection<Account>("Accounts");

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

        private async Task UpdateNetWorthChart()
        {
            var netWorthCalculator = new NetWorthCalculator(_databaseReader);
            var netWorthData = await Task.Run(() =>
                netWorthCalculator.GetNetWorthSinceStartDate(DateTime.Today.AddDays(-30))
            );

            // Convert to a list of DateTimePoint
            List<DateTimePoint> dateTimePoints = [];
            foreach (var kvp in netWorthData)
            {
                dateTimePoints.Add(new DateTimePoint(kvp.Key, (double)kvp.Value));
            }

            TotalNetWorth = new Currency(netWorthData.LastOrDefault().Value);

            var accentColor = ApplicationAccentColorManager.SecondaryAccent;

            NetWorthSeries =
            [
                new LineSeries<DateTimePoint>
                {
                    Values = dateTimePoints,
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint(new SKColor(accentColor.R, accentColor.G, accentColor.B), 2),
                    LineSmoothness = 0,
                }
            ];
        }
    }
}
