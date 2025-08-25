using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;
using SkiaSharp;
using System.Threading.Tasks;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyMoney.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for the reports page, showing various financial reports and charts
    /// </summary>
    public partial class ReportsViewModel : ObservableObject, INavigationAware
    {
        #region Chart Properties

        /// <summary>
        /// Series data for the 12-month income vs expenses chart
        /// </summary>
        [ObservableProperty]
        private ISeries[] _incomeExpense12MonthSeries = [];

        /// <summary>
        /// X-axis configuration for the 12-month chart
        /// </summary>
        [ObservableProperty]
        private Axis[] _incomeExpense12MonthXAxes = GetDefaultXAxis();

        /// <summary>
        /// Y-axis configuration for the 12-month chart
        /// </summary>
        [ObservableProperty]
        private Axis[] _incomeExpense12MonthYAxes = GetDefaultYAxis();

        /// <summary>
        /// Paint configuration for the chart legend
        /// </summary>
        [ObservableProperty]
        private SolidColorPaint _incomeExpense12MonthLegendPaint = new(DefaultTextColor);

        /// <summary>
        /// Current text color for charts, changes with theme
        /// </summary>
        [ObservableProperty]
        private SKColor _chartTextColor = DefaultTextColor;


        [ObservableProperty]
        private Currency _totalNetWorth = new(0m);

        [ObservableProperty]
        private ISeries[] _netWorthSeries = [];

        [ObservableProperty]
        private Axis[] _netWorthYAxes =
        [
            new()
            {
                Labeler = Labelers.Currency,
            }
        ];

        [ObservableProperty]
        private Axis[] _netWorthXAxes =
        [
            new()
            {
                LabelsPaint = null
            }
        ];

        [ObservableProperty]
        private string _netWorthStartDate = "";

        [ObservableProperty]
        private string _netWorthEndDate = "";

        #endregion

        #region Constants

        private static readonly SKColor DefaultTextColor = new(0x33, 0x33, 0x33);
        private static readonly SKColor IncomeFillColor = new(0x21, 0x96, 0xf3);
        private static readonly SKColor ExpenseFillColor = new(0xf4, 0x43, 0x36);
        private const int MaxBarWidth = 25;

        #endregion

        #region Public Methods

        public async Task OnNavigatedToAsync()
        {
            await UpdateCharts();
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        #endregion

        #region Private Methods

        private async Task UpdateCharts()
        {
            UpdateTextColor();
            await UpdateNetWorthChart();
            await Update12MonthIncomeExpenseChart();
        }

        private async Task Update12MonthIncomeExpenseChart()
        {
            var (incomeSeries, expenseSeries) = await Task.Run(GetChartData);
            UpdateChartSeries(incomeSeries, expenseSeries);
            UpdateChartAxes();
            UpdateChartLegend();
        }

        private async Task UpdateNetWorthChart()
        {
            DatabaseManager dbManager = new();
            var netWorthCalculator = new NetWorthCalculator(dbManager);
            var netWorthData = await Task.Run(() => netWorthCalculator.GetNetWorthSinceStartDate(DateTime.Today.AddDays(-30)));
            TotalNetWorth = new Currency(netWorthData.LastOrDefault());
            var accentColor = ApplicationAccentColorManager.SecondaryAccent;
            var lighterAccentColor = ApplicationAccentColorManager.PrimaryAccent;
            NetWorthSeries =
            [
                new LineSeries<decimal>
                {
                    Values = netWorthData,
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint(new SKColor(accentColor.R, accentColor.G, accentColor.B), 4),
                    Fill = new SolidColorPaint(new SKColor(lighterAccentColor.R, lighterAccentColor.G, lighterAccentColor.B, 175)),
                }
            ];

            NetWorthStartDate = DateTime.Today.AddDays(-30).ToString("MMM dd, yyyy");
            NetWorthEndDate = DateTime.Today.ToString("MMM dd, yyyy");
        }

        private static (List<double> income, List<double> expenses) GetChartData()
        {
            return (
                IncomeExpense12MonthCalculator.GetPast12MonthsIncome(),
                IncomeExpense12MonthCalculator.GetPast12MonthsExpenses()
            );
        }

        private void UpdateChartSeries(List<double> incomeSeries, List<double> expenseSeries)
        {
            IncomeExpense12MonthSeries =
            [
                CreateSeries("Income", incomeSeries, IncomeFillColor),
                CreateSeries("Expenses", expenseSeries, ExpenseFillColor)
            ];
        }

        private static ColumnSeries<double> CreateSeries(string name, List<double> values, SKColor fillColor)
        {
            return new ColumnSeries<double>
            {
                Values = values,
                MaxBarWidth = MaxBarWidth,
                Name = name,
                Fill = new SolidColorPaint(fillColor)
            };
        }

        private void UpdateChartAxes()
        {
            var textPaint = new SolidColorPaint(ChartTextColor);
            
            IncomeExpense12MonthXAxes = [CreateMonthAxis(textPaint)];
            IncomeExpense12MonthYAxes = [CreateCurrencyAxis(textPaint)];
            NetWorthYAxes = [CreateNetWorthYAxis(textPaint)];
        }

        private static Axis CreateNetWorthYAxis(SolidColorPaint textPaint)
        {
            return new Axis
            {
                Labeler = Labelers.Currency,
                LabelsPaint = textPaint,
                NamePaint = textPaint
            };
        }

        private static Axis CreateMonthAxis(SolidColorPaint textPaint)
        {
            return new Axis
            {
                Labels = IncomeExpense12MonthCalculator.GetMonthNames(),
                LabelsRotation = 0,
                SeparatorsAtCenter = false,
                TicksAtCenter = true,
                ForceStepToMin = true,
                MinStep = 1,
                LabelsPaint = textPaint,
                NamePaint = textPaint
            };
        }

        private static Axis CreateCurrencyAxis(SolidColorPaint textPaint)
        {
            return new Axis
            {
                Name = "Amount",
                LabelsPaint = textPaint,
                Labeler = Labelers.Currency,
                NamePaint = textPaint,
                MinLimit = 0
            };
        }

        private void UpdateChartLegend()
        {
            IncomeExpense12MonthLegendPaint = new SolidColorPaint(ChartTextColor);
        }

        private void UpdateTextColor()
        {
            ChartTextColor = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light 
                ? DefaultTextColor 
                : new SKColor(0xff, 0xff, 0xff);
        }

        private static Axis[] GetDefaultXAxis()
        {
            return [new Axis
            {
                Labels = [],
                LabelsRotation = 0,
                SeparatorsAtCenter = false,
                TicksAtCenter = true,
                ForceStepToMin = true,
                MinStep = 1,
                LabelsPaint = new SolidColorPaint(DefaultTextColor),
                NamePaint = new SolidColorPaint(DefaultTextColor)
            }];
        }

        private static Axis[] GetDefaultYAxis()
        {
            return [new Axis
            {
                Name = "Amount",
                LabelsPaint = new SolidColorPaint(DefaultTextColor),
                Labeler = Labelers.Currency,
                NamePaint = new SolidColorPaint(DefaultTextColor),
                MinLimit = 0
            }];
        }

        #endregion
    }
}
