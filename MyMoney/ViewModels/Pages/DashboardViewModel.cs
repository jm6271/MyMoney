using System.Collections.ObjectModel;
using MyMoney.Core.Models;
using MyMoney.Core.Reports;
using MyMoney.Core.Database;

namespace MyMoney.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        public ObservableCollection<AccountDashboardDisplayItem> Accounts { get; set; } = [];
        public ObservableCollection<BudgetReportItem> BudgetReportIncomeItems { get; set; } = [];
        public ObservableCollection<BudgetReportItem> BudgetReportExpenseItems { get; set; } = [];

        // Automatically generates the Income property with change notifications
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BarValues))]
        private decimal _income;

        // Automatically generates the Expenses property with change notifications
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BarValues))]
        private decimal _expenses;

        // Computed property for bar values
        public double[] BarValues => [(double)Income, (double)Expenses];

        // Labels for the bars
        public ScottPlot.Tick[] BarLabels = { new(0, "Income"), new(1, "Expenses") };

        // Widths for budget report gridview columns
        [ObservableProperty]
        private int _CategoryColumnWidth = 200;

        [ObservableProperty]
        private int _BudgetedColumnWidth = 100;

        [ObservableProperty]
        private int _ActualColumnWidth = 100;

        [ObservableProperty]
        private int _DifferenceColumnWidth = 100;


        // Values for the budget report totals
        [ObservableProperty]
        private Currency _BudgetedTotal = new();

        [ObservableProperty]
        private Currency _ActualTotal = new();

        [ObservableProperty]
        private Currency _DifferenceTotal = new();

        public DashboardViewModel() 
        {
        }

        private void CalculateBudgetReport()
        {
            // clear the current report
            BudgetReportIncomeItems.Clear();
            BudgetReportExpenseItems.Clear();

            var incomeItems = BudgetReportCalculator.CalculateIncomeReportItems();
            var expenseItems = BudgetReportCalculator.CalculateExpenseReportItems();

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
            Income = incomeTotal.Actual.Value;

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
            Expenses = expenseTotal.Actual.Value;

            // Calulate budget report overall total
            BudgetedTotal = incomeTotal.Budgeted - expenseTotal.Budgeted;
            ActualTotal = incomeTotal.Actual - expenseTotal.Actual;
            DifferenceTotal = ActualTotal - BudgetedTotal;
        }

        public void OnPageNavigatedTo()
        {
            // Reload information from the database
            Accounts.Clear();

            var lst = DatabaseReader.GetCollection<Account>("Accounts");

            foreach (var item in lst)
            {
                Accounts.Add(new(item));
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
