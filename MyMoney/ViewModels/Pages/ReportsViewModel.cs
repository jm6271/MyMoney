using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MyMoney.Core.Reports;
using SkiaSharp;
using Wpf.Ui.Appearance;

namespace MyMoney.ViewModels.Pages
{
    public partial class ReportsViewModel : ObservableObject
    {

        /*********************************************************************/
        /* 12 Month Income vs. Expenses Chart                                */
        /*********************************************************************/

        [ObservableProperty]
        private ISeries[] _incomeExpense12MonthSeries;

        [ObservableProperty]
        private Axis[] _incomeExpense12MonthXAxes;

        [ObservableProperty]
        private Axis[] _incomeExpense12MonthYAxes;

        [ObservableProperty]
        private SolidColorPaint _incomeExpense12MonthLegendPaint;

        /*********************************************************************/


        [ObservableProperty]
        private SKColor _chartTextColor = new(0x33, 0x33, 0x33);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ReportsViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            UpdateCharts();
        }

        [RelayCommand]
        private void OnPageNavigatedTo()
        {
            UpdateCharts();
        }

        private void UpdateCharts()
        {
            UpdateTextColor();

            // 12 Month Income Expense Chart
            Update12MonthIncomeExpenseChart();
        }

        private void Update12MonthIncomeExpenseChart()
        {
            // Set series
            var incomeSeries = IncomeExpense12MonthCalculator.GetPast12MonthsIncome();
            var expenseSeries = IncomeExpense12MonthCalculator.GetPast12MonthsExpenses();

            IncomeExpense12MonthSeries =
            [
                new ColumnSeries<double>
                {
                    Values = incomeSeries,
                    MaxBarWidth = 25,
                    Name = "Income",
                    Fill = new SolidColorPaint(new SKColor(0x21, 0x96, 0xf3))
                },
                new ColumnSeries<double>
                {
                    Values = expenseSeries,
                    MaxBarWidth = 25,
                    Name = "Expenses",
                    Fill = new SolidColorPaint(new SKColor(0xf4, 0x43, 0x36)),
                },
            ];

            // Set XAxis
            IncomeExpense12MonthXAxes =
            [
                new Axis
                {
                    Labels = IncomeExpense12MonthCalculator.GetMonthNames(),
                    LabelsRotation = 0,
                    SeparatorsAtCenter = false,
                    TicksAtCenter = true,
                    ForceStepToMin = true,
                    MinStep = 1,
                    LabelsPaint = new SolidColorPaint(ChartTextColor),
                    NamePaint = new SolidColorPaint(ChartTextColor),
                }
            ];

            // Set YAxis
            IncomeExpense12MonthYAxes =
            [
                new Axis
                {
                    Name = "Amount",
                    LabelsPaint = new SolidColorPaint(ChartTextColor),
                    NamePaint = new SolidColorPaint(ChartTextColor),
                }
            ];

            // Set legend color
            IncomeExpense12MonthLegendPaint = new SolidColorPaint(ChartTextColor);

        }

        private void UpdateTextColor()
        {
            ChartTextColor = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light 
                ? new SKColor(0x33, 0x33, 0x33) 
                : new SKColor(0xff, 0xff, 0xff);
        }
    }
}
