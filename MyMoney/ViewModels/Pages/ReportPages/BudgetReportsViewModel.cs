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

        public void OnPageNavigatedTo()
        {
            CalculateReport();
        }

        private void CalculateReport()
        {
            // clear the current report
            IncomeItems.Clear();
            ExpenseItems.Clear();

            var incomeItems = BudgetReportCalculator.CalculateIncomeReportItems();
            var expenseItems = BudgetReportCalculator.CalculateExpenseReportItems();

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
    }
}
