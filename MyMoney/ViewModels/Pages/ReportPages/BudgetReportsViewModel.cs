using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;
using SkiaSharp;
using System.Collections.ObjectModel;
using Wpf.Ui.Appearance;

namespace MyMoney.ViewModels.Pages.ReportPages
{
    public partial class BudgetReportsViewModel(IDatabaseReader databaseReader) : ObservableObject
    {
        [ObservableProperty]
        private string _reportTitle = "Budget Report";

        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _incomeItems = [];

        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _expenseItems = [];

        [ObservableProperty]
        private Currency _reportTotal = new(0m);

        [ObservableProperty]
        private ObservableCollection<Budget> _budgets = [];

        [ObservableProperty]
        private Budget? _selectedBudget;

        [ObservableProperty]
        private int _selectedBudgetIndex;

        [ObservableProperty]
        private ISeries[] _actualIncomeSeries = [];

        [ObservableProperty]
        private LabelVisual _actualIncomeTitle = new()
        {
            Text = "Actual Income",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        [ObservableProperty]
        private ISeries[] _actualExpenseSeries = [];

        [ObservableProperty]
        private LabelVisual _actualExpensesTitle = new()
        {
            Text = "Actual Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        // Colors for chart text (changes in light and dark modes)
        [ObservableProperty]
        private SKColor _chartTextColor = new(0x33, 0x33, 0x33);

        public void OnPageNavigatedTo()
        {
            LoadBudgets();
            UpdateCharts();
        }

        partial void OnSelectedBudgetChanged(Budget? value)
        {
            // Load the new budget report
            // Make sure a budget is selected
            if (value == null)
            {
                ReportTitle = "Budget Report";
                return;
            }

            CalculateReport(value.BudgetDate);

            // Set the report title
            ReportTitle = value.BudgetTitle;

            // Update the charts
            UpdateCharts();
        }

        private void LoadBudgets()
        {
            // Read all the budgets from the database
            BudgetCollection budgetCollection = new(databaseReader);
            ObservableCollection<Budget> unsortedBudgets = [.. budgetCollection.Budgets];

            // Sort the budgets
            Budgets = [.. unsortedBudgets.OrderByDescending(o => o.BudgetDate)];

            if (Budgets.Count > 0)
                SelectedBudgetIndex = 0;
        }

        public void CalculateReport(DateTime date)
        {
            // clear the current report
            IncomeItems.Clear();
            ExpenseItems.Clear();

            var incomeItems = BudgetReportCalculator.CalculateIncomeReportItems(date, databaseReader);
            var expenseItems = BudgetReportCalculator.CalculateExpenseReportItems(date, databaseReader);

            foreach (var item in incomeItems)
            {
                IncomeItems.Add(item);
            }

            foreach (var item in expenseItems)
            {
                ExpenseItems.Add(item);
            }

            // Add an item to the income list showing the total income
            BudgetReportItem incomeTotal = new();

            foreach (var item in IncomeItems)
            {
                incomeTotal.Actual += item.Actual;
                incomeTotal.Budgeted += item.Budgeted;
                incomeTotal.Remaining += item.Remaining;
            }

            incomeTotal.Category = "Total";
            IncomeItems.Add(incomeTotal);

            // Add an item to the expense list showing the total expenses
            BudgetReportItem expenseTotal = new();

            foreach (var item in ExpenseItems)
            {
                expenseTotal.Actual += item.Actual;
                expenseTotal.Budgeted += item.Budgeted;
                expenseTotal.Remaining += item.Remaining;
            }

            expenseTotal.Category = "Total";
            ExpenseItems.Add(expenseTotal);

            // Calculate budget report overall total
            ReportTotal = incomeTotal.Actual - expenseTotal.Actual;
        }

        private void UpdateCharts()
        {
            UpdateActualIncomeChart();
            UpdateActualExpensesChart();
            UpdateChartsTheme();
        }

        private void UpdateActualIncomeChart()
        {
            if (SelectedBudget == null)
                return;

            Dictionary<string, double> incomeTotals = [];
            for (var j = 0; j < IncomeItems.Count - 1; j++)
            {
                if (IncomeItems[j].Actual.Value == 0m) continue;

                incomeTotals.Add(IncomeItems[j].Category, (double)IncomeItems[j].Actual.Value);
            }

            ActualIncomeSeries = new ISeries[incomeTotals.Count];
            var i = 0;
            foreach (var item in incomeTotals)
            {
                ActualIncomeSeries[i] = new PieSeries<double> { Values = [item.Value], Name = item.Key };
                i++;
            }
        }

        private void UpdateActualExpensesChart()
        {
            if (SelectedBudget == null)
                return;

            Dictionary<string, double> expenseTotals = [];
            for (var j = 0; j < ExpenseItems.Count - 1; j++)
            {
                if (ExpenseItems[j].Actual.Value == 0m) continue;

                expenseTotals.Add(ExpenseItems[j].Category, (double)ExpenseItems[j].Actual.Value);
            }

            ActualExpenseSeries = new ISeries[expenseTotals.Count];
            var i = 0;
            foreach (var item in expenseTotals)
            {
                ActualExpenseSeries[i] = new PieSeries<double> { Values = [item.Value], Name = item.Key };
                i++;
            }
        }

        private void UpdateChartsTheme()
        {
            ChartTextColor = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light 
                ? new SKColor(0x33, 0x33, 0x33) 
                : new SKColor(0xff, 0xff, 0xff);

            ActualIncomeTitle.Paint = new SolidColorPaint(ChartTextColor);
            ActualExpensesTitle.Paint = new SolidColorPaint(ChartTextColor);
        }
    }
}
