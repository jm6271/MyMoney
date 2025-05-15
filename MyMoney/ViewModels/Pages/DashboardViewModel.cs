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
    public partial class DashboardViewModel : ObservableObject
    {
        public ObservableCollection<AccountDashboardDisplayItem> Accounts { get; set; } = [];
        public ObservableCollection<BudgetReportItem> BudgetReportIncomeItems { get; set; } = [];
        public ObservableCollection<BudgetReportItem> BudgetReportExpenseItems { get; set; } = [];

        [ObservableProperty]
        private double _income;

        [ObservableProperty]
        private double _expenses;

        [ObservableProperty]
        private ISeries[] _series;

        // Widths for budget report gridview columns
        [ObservableProperty]
        private int _categoryColumnWidth = 200;

        [ObservableProperty]
        private int _budgetedColumnWidth = 100;

        [ObservableProperty]
        private int _actualColumnWidth = 100;

        [ObservableProperty]
        private int _differenceColumnWidth = 100;

        [ObservableProperty]
        private Currency _differenceTotal = new(0m);

        // Axis for the chart
        [ObservableProperty]
        private Axis[] _xAxes  =
        [
        new ()
        {
            Labels = ["Income", "Expenses"],
            LabelsRotation = 0,
            TicksAtCenter = true,
            // By default, the axis tries to optimize the number of 
            // labels to fit the available space, 
            // when you need to force the axis to show all the labels then you must: 
            ForceStepToMin = true,
            MinStep = 1
        }
        ];

        [ObservableProperty]
        private Axis[] _yAxes =
        [
            new ()
            {
                Name = "Amount",
                Labeler = Labelers.Currency,
            }
        ];

        // Chart title
        [ObservableProperty]
        private LabelVisual _chartTitle = new()
        {
            Text = "Income vs. Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        // Colors for text (changes in light and dark modes)
        [ObservableProperty]
        private SKColor _chartTextColor = new(0x33, 0x33, 0x33);

        // Service for reading from the database
        readonly IDatabaseReader _databaseReader;

        public DashboardViewModel(IDatabaseReader databaseReader)
        {
            Series = UpdateChartSeries();
            _databaseReader = databaseReader;
        }

        private void CalculateBudgetReport()
        {
            // clear the current report
            BudgetReportIncomeItems.Clear();
            BudgetReportExpenseItems.Clear();

            var incomeItems = BudgetReportCalculator.CalculateIncomeReportItems(_databaseReader);
            var expenseItems = BudgetReportCalculator.CalculateExpenseReportItems(_databaseReader);

            foreach (var item in incomeItems)
            {
                BudgetReportIncomeItems.Add(item);
            }

            foreach (var item in expenseItems)
            {
                BudgetReportExpenseItems.Add(item);
            }

            // Add an item to the income list showing the total income
            BudgetReportItem incomeTotal = new();

            foreach (var item in BudgetReportIncomeItems)
            {
                incomeTotal.Actual += item.Actual;
                incomeTotal.Budgeted += item.Budgeted;
                incomeTotal.Remaining += item.Remaining;
            }

            incomeTotal.Category = "Total";
            BudgetReportIncomeItems.Add(incomeTotal);
            Income = (double)incomeTotal.Actual.Value;

            // Add an item to the expense list showing the total expenses
            BudgetReportItem expenseTotal = new();

            foreach (var item in BudgetReportExpenseItems)
            {
                expenseTotal.Actual += item.Actual;
                expenseTotal.Budgeted += item.Budgeted;
                expenseTotal.Remaining += item.Remaining;
            }

            expenseTotal.Category = "Total";
            BudgetReportExpenseItems.Add(expenseTotal);
            Expenses = (double)expenseTotal.Actual.Value;

            // Calculate budget report overall total
            DifferenceTotal = incomeTotal.Actual - expenseTotal.Actual;


            // update the chart series
            Series = UpdateChartSeries();

            // Update the chart theme
            UpdateChartTheme();

        }

        private void UpdateChartTheme()
        {
            ChartTextColor = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light 
                ? new SKColor(0x33, 0x33, 0x33) 
                : new SKColor(0xff, 0xff, 0xff);

            XAxes[0].LabelsPaint = new SolidColorPaint(ChartTextColor);
            YAxes[0].LabelsPaint = new SolidColorPaint(ChartTextColor);
            YAxes[0].NamePaint = new SolidColorPaint(ChartTextColor);
            ChartTitle.Paint = new SolidColorPaint(ChartTextColor);
        }

        private ISeries[] UpdateChartSeries()
        {
            var accentColor = ApplicationAccentColorManager.GetColorizationColor();
            ISeries[] s = [new ColumnSeries<double>()
            {
                Values = [Income, Expenses],
                Fill = new SolidColorPaint(new SKColor(accentColor.R, accentColor.G, accentColor.B)),
                Stroke = null,
            }];
            return s;
        }

        public void OnPageNavigatedTo()
        {
            // Reload information from the database
            Accounts.Clear();

            var lst = _databaseReader.GetCollection<Account>("Accounts");

            foreach (var accountDisplayItem in lst.Select(AccountDashboardDisplayItem.FromAccount))
            {
                Accounts.Add(accountDisplayItem);
            }

            // add an item displaying the total as the last item in the list
            AccountDashboardDisplayItem totalItem = new()
            {
                AccountName = "Total"
            };
            foreach (var account in Accounts)
            {
                totalItem.Total += account.Total;
            }

            Accounts.Add(totalItem);

            CalculateBudgetReport();
        }
    }
}
