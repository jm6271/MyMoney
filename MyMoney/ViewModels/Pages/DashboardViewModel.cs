using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Painting;
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

        #region Income vs. Expense Chart Properties

        [ObservableProperty]
        private ISeries[] _series = [];

        [ObservableProperty]
        private SKColor _chartTextColor = new(0x33, 0x33, 0x33);

        [ObservableProperty]
        private LabelVisual _chartTitle = new()
        {
            Text = "Income vs. Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
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
                MinStep = 1,
            },
        ];

        [ObservableProperty]
        private Axis[] _yAxes = [new() { Labeler = Labelers.Currency }];

        #endregion

        #region Expense breakdown chart properties

        [ObservableProperty]
        private ISeries[] _expensePercentagesSeries = [];

        [ObservableProperty]
        private Paint _expenseBreakdownForeColor = new SolidColorPaint();

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
            new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM dd")) { LabelsPaint = null },
        ];

        [ObservableProperty]
        private Axis[] _netWorthYAxes = [new() { LabelsPaint = null, ShowSeparatorLines = false }];

        #endregion

        #region Cash Flow Card Properties

        [ObservableProperty]
        private Currency _cashFlowTotal = new(0m);

        [ObservableProperty]
        private Currency _lastMonthCashFlowTotal = new(0m);

        public double CashFlowPercentVsLastMonth
        {
            get
            {
                if (LastMonthCashFlowTotal.Value == 0m)
                {
                    return double.PositiveInfinity;
                }
                var percentage = Math.Round(
                    (double)((CashFlowTotal.Value - LastMonthCashFlowTotal.Value) / Math.Abs(LastMonthCashFlowTotal.Value) * 100),
                    1
                );

                return percentage;
            }
        }

        public string CashFlowTotalFormatted
        {
            get
            {
                if (CashFlowTotal.Value > 0)
                {
                    return "+" + CashFlowTotal.ToString();
                }
                return CashFlowTotal.ToString();
            }
        }

        public Brush CashFlowTotalColorBrush
        {
            get
            {
                if (CashFlowTotal.Value > 0)
                    return ApplicationAccentColorManager.PrimaryAccentBrush;
                else if (CashFlowTotal.Value < 0)
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x55, 0x55));
                else
                    return (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            }
        }

        public Brush CashFlowPercentageColorBrush
        {
            get
            {
                if (CashFlowPercentVsLastMonth > 0d)
                    return ApplicationAccentColorManager.PrimaryAccentBrush;
                else if (CashFlowPercentVsLastMonth < 0d)
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x55, 0x55));
                else
                    return (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            }
        }

        #endregion

        #region Budget Health Card Properties

        [ObservableProperty]
        SymbolRegular _budgetHealthIcon = SymbolRegular.CheckmarkCircle24;

        [ObservableProperty]
        private Brush _budgetHealthIconColorBrush = ApplicationAccentColorManager.PrimaryAccentBrush;

        public string BudgetHealthText
        {
            get
            {
                List<BudgetReportItem> overspentItems = [];
                foreach (var item in BudgetReportExpenseItems)
                {
                    if (item.Actual.Value > item.Budgeted.Value)
                    {
                        overspentItems.Add(item);
                    }
                }

                if (overspentItems.Count == 0)
                {
                    BudgetHealthIcon = SymbolRegular.CheckmarkCircle24;
                    BudgetHealthIconColorBrush = ApplicationAccentColorManager.PrimaryAccentBrush;
                    return "You're on track this month";
                }
                else
                {
                    BudgetHealthIcon = SymbolRegular.Warning24;
                    BudgetHealthIconColorBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xAA, 0x00));
                    if (overspentItems.Count == 1)
                    {
                        return $"You're over budget in {overspentItems[0].Category}";
                    }
                    else
                    {
                        return $"You're over budget in {overspentItems[0].Category} and {overspentItems.Count - 1} more";
                    }
                }
            }
        }

        #endregion

        #region Spending Insights Card Properties

        [ObservableProperty]
        private Currency _totalSpending = new(0m);

        [ObservableProperty]
        private Currency _lastMonthTotalSpending = new(0m);

        public double SpendingVsLastMonthPercentage
        {
            get
            {

                if (LastMonthTotalSpending.Value == 0m)
                {
                    return double.PositiveInfinity;
                }

                var percentage = Math.Round(
                    (double)(
                        (TotalSpending.Value - LastMonthTotalSpending.Value)
                        / Math.Abs(LastMonthTotalSpending.Value)
                        * 100
                    ),
                    1
                );
                return percentage;
            }
        }

        public Brush SpendingPercentageColorBrush
        {
            get
            {
                if (SpendingVsLastMonthPercentage < 0d)
                    return ApplicationAccentColorManager.PrimaryAccentBrush;
                else if (SpendingVsLastMonthPercentage > 0d)
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x55, 0x55));
                else
                    return (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            }
        }

        #endregion

        private readonly IDatabaseManager _databaseReader;
        private readonly Lock _incomeItemsLock = new();
        private readonly Lock _expenseItemsLock = new();
        private readonly Lock _savingsItemsLock = new();
        private readonly Lock _reportItemsLock = new();

        public DashboardViewModel(IDatabaseManager databaseReader)
        {
            _databaseReader = databaseReader ?? throw new ArgumentNullException(nameof(databaseReader));
        }

        private async Task CalculateBudgetReport()
        {
            ClearReports();
            var reportItems = await LoadReportItems();
            UpdateReportCollections(reportItems);
            await UpdateChartDisplay();

            CashFlowTotal = await GetCashFlowTotal(DateTime.Now);
            LastMonthCashFlowTotal = await GetCashFlowTotal(DateTime.Now.AddMonths(-1));
            OnPropertyChanged(nameof(CashFlowPercentVsLastMonth));
            OnPropertyChanged(nameof(CashFlowTotalFormatted));
            OnPropertyChanged(nameof(CashFlowTotalColorBrush));
            OnPropertyChanged(nameof(CashFlowPercentageColorBrush));

            OnPropertyChanged(nameof(BudgetHealthText));

            TotalSpending = await GetSpendingTotal(DateTime.Now);
            LastMonthTotalSpending = await GetSpendingTotal(DateTime.Now.AddMonths(-1));
            OnPropertyChanged(nameof(SpendingVsLastMonthPercentage));
            OnPropertyChanged(nameof(SpendingPercentageColorBrush));
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

        private async Task<(
            List<BudgetReportItem> income,
            List<BudgetReportItem> expense,
            List<SavingsCategoryReportItem> savings
        )> LoadReportItems()
        {
            var reportItems = await BudgetReportCalculator.CalculateBudgetReport(DateTime.Today, _databaseReader);
            var incomeTotal = CalculateTotal(reportItems.income);
            var expenseTotal = CalculateTotal(reportItems.expenses);
            //reportItems.income.Add(incomeTotal);
            //reportItems.expenses.Add(expenseTotal);

            Income = (double)incomeTotal.Actual.Value;
            Expenses = (double)expenseTotal.Actual.Value;
            DifferenceTotal = incomeTotal.Actual - expenseTotal.Actual;

            return reportItems;
        }

        private void UpdateReportCollections(
            (
                List<BudgetReportItem> income,
                List<BudgetReportItem> expense,
                List<SavingsCategoryReportItem> savings
            ) items
        )
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
                        IsExpense = false,
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
            Series = await UpdateChartSeries();
            ExpensePercentagesSeries = await UpdateExpenseBreakdownSeries();
            await UpdateNetWorthChart();
            UpdateChartTheme();
        }

        private async Task<ISeries[]> UpdateChartSeries()
        {
            var past12MonthsIncome = await IncomeExpense12MonthCalculator.GetPast12MonthsIncome();
            var past12MonthsExpenses = await IncomeExpense12MonthCalculator.GetPast12MonthsExpenses();

            var incomeTotal = past12MonthsIncome.Count > 0 ? past12MonthsIncome[^1] : 0.0;
            var expenseTotal = past12MonthsExpenses.Count > 0 ? past12MonthsExpenses[^1] : 0.0;

            var accentColor = ApplicationAccentColorManager.PrimaryAccent;
            return
            [
                new ColumnSeries<double>
                {
                    Values = [incomeTotal, expenseTotal],
                    Fill = new SolidColorPaint(new SKColor(accentColor.R, accentColor.G, accentColor.B)),
                    Stroke = null,
                },
            ];
        }

        private async Task<ISeries[]> UpdateExpenseBreakdownSeries()
        {
            // Get expense categories
            var expenseCategories = await BudgetReportCalculator.CalculateExpenseReportItems(DateTime.Now, _databaseReader);

            // Include savings with expenses
            var savingsCategories = await BudgetReportCalculator.CalculateSavingsReportItems(DateTime.Now, _databaseReader);
            foreach (var savings in savingsCategories)
            {
                expenseCategories.Add(new BudgetReportItem { Category = savings.Category, Actual = savings.Saved });
            }

            var expenseTotals = GetNonZeroTotals(expenseCategories, item => item.Actual.Value, item => item.Category);

            return CreatePieSeries(expenseTotals);
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
            return
            [
                .. totals.Select(item => new PieSeries<double>
                {
                    Values = [item.Value],
                    Name = item.Key,
                    DataLabelsFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                    ToolTipLabelFormatter = point => point.Model.ToString("C", CultureInfo.CurrentCulture),
                    MaxRadialColumnWidth = 25,
                }),
            ];
        }

        private void UpdateChartTheme()
        {
            ChartTextColor =
                ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light
                    ? new SKColor(0x33, 0x33, 0x33)
                    : new SKColor(0xff, 0xff, 0xff);

            var textPaint = new SolidColorPaint(ChartTextColor);
            XAxes[0].LabelsPaint = textPaint;
            YAxes[0].LabelsPaint = textPaint;
            YAxes[0].NamePaint = textPaint;
            ChartTitle.Paint = textPaint;

            ExpenseBreakdownForeColor =
                ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light
                    ? new SolidColorPaint(new SKColor(0x33, 0x33, 0x33))
                    : new SolidColorPaint(new SKColor(0xff, 0xff, 0xff));
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
            var totalItem = new AccountDashboardDisplayItem { AccountName = "Total" };

            foreach (var account in Accounts)
            {
                totalItem.Total += account.Total;
            }

            Accounts.Add(totalItem);
        }

        private async Task UpdateNetWorthChart()
        {
            var netWorthCalculator = new NetWorthCalculator(_databaseReader);
            var netWorthData = await netWorthCalculator.GetNetWorthSinceStartDate(DateTime.Today.AddDays(-30));

            // Convert to a list of DateTimePoint
            List<DateTimePoint> dateTimePoints = [];
            foreach (var kvp in netWorthData)
            {
                dateTimePoints.Add(new DateTimePoint(kvp.Key, (double)kvp.Value));
            }

            TotalNetWorth = new Currency(netWorthData.LastOrDefault().Value);

            var accentColor = ApplicationAccentColorManager.SecondaryAccent;
            var lighterAccentColor = ApplicationAccentColorManager.PrimaryAccent;

            NetWorthSeries =
            [
                new LineSeries<DateTimePoint>
                {
                    Values = dateTimePoints,
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint(new SKColor(accentColor.R, accentColor.G, accentColor.B), 2),
                    LineSmoothness = 0,
                    Fill = new SolidColorPaint(
                        new SKColor(lighterAccentColor.R, lighterAccentColor.G, lighterAccentColor.B, 100)
                    ),
                },
            ];
        }

        private async Task<Currency> GetCashFlowTotal(DateTime month)
        {
            // Get a budget report for the specified month
            var report = await BudgetReportCalculator.CalculateBudgetReport(month, _databaseReader);

            Currency income = new(0m);
            foreach (var item in report.income)
            {
                income.Value += item.Actual.Value;
            }

            Currency expense = new(0m);
            foreach (var item in report.expenses)
            {
                expense.Value += item.Actual.Value;
            }

            return income - expense;
        }

        private async Task<Currency> GetSpendingTotal(DateTime month)
        {
            var report = await BudgetReportCalculator.CalculateExpenseReportItems(month, _databaseReader);
            Currency totalSpending = new(0m);
            foreach (var item in report)
            {
                totalSpending.Value += item.Actual.Value;
            }
            return totalSpending;
        }
    }
}
