using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private ISeries[] _IncomeExpense12Month_Series;

        [ObservableProperty]
        private Axis[] _IncomeExpense12Month_XAxes;

        [ObservableProperty]
        private Axis[] _IncomeExpense12Month_YAxes;

        [ObservableProperty]
        private SolidColorPaint _IncomeExpense12Month_LegendPaint;

        /*********************************************************************/


        [ObservableProperty]
        private SKColor _ChartTextColor = new(0x33, 0x33, 0x33);

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
            var IncomeSeries = IncomeExpense12MonthCalculator.GetPast12MonthsIncome();
            var ExpenseSeries = IncomeExpense12MonthCalculator.GetPast12MonthsExpenses();

            IncomeExpense12Month_Series =
            [
                new ColumnSeries<double>
                {
                    Values = IncomeSeries,
                    MaxBarWidth = 25,
                    Name = "Income",
                    Fill = new SolidColorPaint(new SKColor(0x21, 0x96, 0xf3))
                },
                new ColumnSeries<double>
                {
                    Values = ExpenseSeries,
                    MaxBarWidth = 25,
                    Name = "Expenses",
                    Fill = new SolidColorPaint(new SKColor(0xf4, 0x43, 0x36)),
                },
            ];

            // Set XAxis
            IncomeExpense12Month_XAxes =
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
            IncomeExpense12Month_YAxes =
            [
                new Axis
                {
                    Name = "Amount",
                    LabelsPaint = new SolidColorPaint(ChartTextColor),
                    NamePaint = new SolidColorPaint(ChartTextColor),
                }
            ];

            // Set legend color
            IncomeExpense12Month_LegendPaint = new SolidColorPaint(ChartTextColor);

        }

        private void UpdateTextColor()
        {
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light)
            {
                ChartTextColor = new SKColor(0x33, 0x33, 0x33);
            }
            else
            {
                ChartTextColor = new SKColor(0xff, 0xff, 0xff);
            }
        }
    }
}
