﻿using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using MyMoney.Core.Database;
using MyMoney.Core.FS.Models;
using MyMoney.Core.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.ViewModels.Pages.ReportPages
{
    public partial class BudgetReportsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _reportTitle = "Budget Report";

        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _IncomeItems = [];

        [ObservableProperty]
        private ObservableCollection<BudgetReportItem> _ExpenseItems = [];

        [ObservableProperty]
        private Currency _ReportTotal = new(0m);

        [ObservableProperty]
        private ObservableCollection<Budget> _Budgets = [];

        [ObservableProperty]
        private Budget? _SelectedBudget = null;

        [ObservableProperty]
        private int _SelectedBudgetIndex = 0;

        [ObservableProperty]
        private ISeries[] _ActualIncomeSeries = [];

        [ObservableProperty]
        private LabelVisual _ActualIncome_Title = new()
        {
            Text = "Actual Income",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        [ObservableProperty]
        private ISeries[] _ActualExpenseSeries = [];

        [ObservableProperty]
        private LabelVisual _ActualExpenses_Title = new()
        {
            Text = "Actual Expenses",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

        private readonly IDatabaseReader _DatabaseReader;
        public BudgetReportsViewModel(IDatabaseReader databaseReader)
        {
            _DatabaseReader = databaseReader;
        }

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
        }

        private void LoadBudgets()
        {
            // Read all the budgets from the database
            BudgetCollection budgetCollection = new(_DatabaseReader);
            ObservableCollection<Budget> unsortedBudgets = [.. budgetCollection.Budgets];

            // Sort the budgets
            Budgets = [.. unsortedBudgets.OrderByDescending(o => o.BudgetDate)];

            if (Budgets.Count > 0)
                SelectedBudgetIndex = 0;
        }

        private void CalculateReport(DateTime date)
        {
            // clear the current report
            IncomeItems.Clear();
            ExpenseItems.Clear();

            var incomeItems = BudgetReportCalculator.CalculateIncomeReportItems(date);
            var expenseItems = BudgetReportCalculator.CalculateExpenseReportItems(date);

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

            // Calulate budget report overall total
            Currency BudgetedTotal = incomeTotal.Budgeted - expenseTotal.Budgeted;
            Currency ActualTotal = incomeTotal.Actual - expenseTotal.Actual;
            ReportTotal = ActualTotal - BudgetedTotal;
        }

        private void UpdateCharts()
        {
            UpdateActualIncomeChart();
            UpdateActualExpensesChart();
        }

        private void UpdateActualIncomeChart()
        {
            if (SelectedBudget == null)
                return;

            Dictionary<string, double> incomeTotals = [];
            for (int j = 0; j < IncomeItems.Count - 1; j++)
            {
                if (IncomeItems[j].Actual.Value == 0m) continue;

                incomeTotals.Add(IncomeItems[j].Category, (double)IncomeItems[j].Actual.Value);
            }

            ActualIncomeSeries = new ISeries[incomeTotals.Count];
            int i = 0;
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
            for (int j = 0; j < ExpenseItems.Count - 1; j++)
            {
                if (ExpenseItems[j].Actual.Value == 0m) continue;

                expenseTotals.Add(ExpenseItems[j].Category, (double)ExpenseItems[j].Actual.Value);
            }

            ActualExpenseSeries = new ISeries[expenseTotals.Count];
            int i = 0;
            foreach (var item in expenseTotals)
            {
                ActualExpenseSeries[i] = new PieSeries<double> { Values = [item.Value], Name = item.Key };
                i++;
            }
        }
    }
}
