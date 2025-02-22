using System.Collections.ObjectModel;
using MyMoney.Core.FS.Models;
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
        private ISeries[] _Series;

        // Widths for budget report gridview columns
        [ObservableProperty]
        private int _CategoryColumnWidth = 200;

        [ObservableProperty]
        private int _BudgetedColumnWidth = 100;

        [ObservableProperty]
        private int _ActualColumnWidth = 100;

        [ObservableProperty]
        private int _DifferenceColumnWidth = 100;

        [ObservableProperty]
        private Currency _DifferenceTotal = new(0m);

        // Axis for the chart
        [ObservableProperty]
        private Axis[] _XAxes  =
        [
        new Axis
        {
            Labels = ["Income", "Expenses"],
            LabelsRotation = 0,
            TicksAtCenter = true,
            // By default the axis tries to optimize the number of 
            // labels to fit the available space, 
            // when you need to force the axis to show all the labels then you must: 
            ForceStepToMin = true,
            MinStep = 1
        }
        ];

        [ObservableProperty]
        private Axis[] _YAxes =
        [
            new Axis
            {
                Name = "Amount"
            }
        ];

        // Chart title
        [ObservableProperty]
        private LabelVisual _ChartTitle = new LabelVisual
        {
            Text = "Income vs. Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        // Colors for text (changes in light and dark modes)
        [ObservableProperty]
        private SKColor _ChartTextColor = new(0x33, 0x33, 0x33);

        // Service for reading from the database
        IDatabaseReader databaseReader;

        public DashboardViewModel(IDatabaseReader databaseReader)
        {
            Series = UpdateChartSeries();
            this.databaseReader = databaseReader;
        }

        private void CalculateBudgetReport()
        {
            // clear the current report
            BudgetReportIncomeItems.Clear();
            BudgetReportExpenseItems.Clear();

            var incomeItems = BudgetReportCalculator.CalculateIncomeReportItems(databaseReader);
            var expenseItems = BudgetReportCalculator.CalculateExpenseReportItems(databaseReader);

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

            // Calulate budget report overall total
            Currency BudgetedTotal = incomeTotal.Budgeted - expenseTotal.Budgeted;
            Currency ActualTotal = incomeTotal.Actual - expenseTotal.Actual;
            DifferenceTotal = ActualTotal - BudgetedTotal;


            // update the chart series
            Series = UpdateChartSeries();

            // Update the chart theme
            UpdateChartTheme();

        }

        private void UpdateChartTheme()
        {
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light)
            {
                ChartTextColor = new SKColor(0x33, 0x33, 0x33);
            }
            else
            {
                ChartTextColor = new SKColor(0xff, 0xff, 0xff);
            }

            XAxes[0].LabelsPaint = new SolidColorPaint(ChartTextColor);
            YAxes[0].LabelsPaint = new SolidColorPaint(ChartTextColor);
            YAxes[0].NamePaint = new SolidColorPaint(ChartTextColor);
            ChartTitle.Paint = new SolidColorPaint(ChartTextColor);
        }

        private ISeries[] UpdateChartSeries()
        {
            var AccentColor = ApplicationAccentColorManager.GetColorizationColor();
            ISeries[] s = [new ColumnSeries<double>()
            {
                Values = [Income, Expenses],
                Fill = new SolidColorPaint(new SKColor(AccentColor.R, AccentColor.G, AccentColor.B)),
                Stroke = null,
            }];
            return s;
        }

        public void OnPageNavigatedTo()
        {
            // Reload information from the database
            Accounts.Clear();

            var lst = databaseReader.GetCollection<Account>("Accounts");

            foreach (var item in lst)
            {
                var accountDisplayItem = AccountDashboardDisplayItem.FromAccount(item);
                Accounts.Add(accountDisplayItem);
            }

            // add an item displaying the total as the last item in the list
            AccountDashboardDisplayItem totalItem = new();
            totalItem.AccountName = "Total";
            foreach (var account in Accounts)
            {
                totalItem.Total += account.Total;
            }

            Accounts.Add(totalItem);

            CalculateBudgetReport();
        }
    }
}
