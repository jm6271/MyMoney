using LiteDB;
using MyMoney.Models;
using System.Collections.ObjectModel;
using System.Windows.Media;

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

            var incomeItems = CalculateReportItems("BudgetIncomeItems", false);
            var expenseItems = CalculateReportItems("BudgetExpenseItems", true);

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

        private List<BudgetReportItem> CalculateReportItems(string itemsCollectionName = "BudgetIncomeItems", bool Expense = false)
        {
            List<BudgetReportItem> result = new();

            using (var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath()))
            {
                var BudgetItemsList = db.GetCollection<BudgetIncomeItem>(itemsCollectionName);


                // load the items collection
                for (int i = 1; i <= BudgetItemsList.Count(); i++)
                {
                    var BudgetItem = BudgetItemsList.FindById(i);

                    BudgetReportItem itm = new();
                    itm.Category = BudgetItem.Category;
                    itm.Budgeted = BudgetItem.Amount;

                    // calculate how much was actually spen out of this category

                    // Load the accounts and search the transactions since the beginning of the month
                    var AccountsList = db.GetCollection<Account>("Accounts");
                    List<Account> accounts = new();

                    // iterate over the accounts in the database and add them to the Accounts collection
                    for (int j = 1; j <= AccountsList.Count(); j++)
                    {
                        var account = AccountsList.FindById(j);

                        accounts.Add(account);
                    }

                    // search through each account for transactions in this category that happened this month

                    // Total for this category so far
                    decimal Actual = 0m;

                    foreach (var account in accounts)
                    {
                        foreach (var transaction in account.Transactions)
                        {
                            if (transaction.Category == itm.Category && IsDateInCurrentMonth(transaction.Date))
                            {
                                if (Expense)
                                {
                                    Actual += transaction.Spend.Value;
                                }
                                else
                                {
                                    Actual += transaction.Receive.Value;
                                }
                                
                            }
                        }
                    }

                    itm.Actual = new(Actual);

                    if (Expense)
                        itm.Remaining = itm.Budgeted - itm.Actual;
                    else
                        itm.Remaining = new(InvertSign((itm.Budgeted - itm.Actual).Value));

                    result.Add(itm);
                }
            }

            return result;
        }

        private bool IsDateInCurrentMonth(DateTime date)
        {
            DateTime currentDate = DateTime.Now;
            return date.Year == currentDate.Year && date.Month == currentDate.Month;
        }

        private decimal InvertSign(decimal amount)
        {
            if (amount < 0)
                return Math.Abs(amount);
            var temp = amount;
            amount -= temp * 2;
            return amount;
        }


        public void OnPageNavigatedTo()
        {
            // Reload information from the database
            Accounts.Clear();

            using (var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath()))
            {
                // load the accounts list
                var AccountsList = db.GetCollection<Account>("Accounts");

                // iterate over the accounts in the database and add them to the Accounts collection
                for (int i = 1; i <= AccountsList.Count(); i++)
                {
                    var account = AccountsList.FindById(i);

                    // Convert to an AccountDashboardDisplayItem and add to accounts list

                    Accounts.Add(new(account));
                }

                // add an item displaying the total as the last item in the list
                AccountDashboardDisplayItem totalItem = new();
                totalItem.AccountName = "Total";
                foreach (var account in Accounts)
                {
                    totalItem.Total += account.Total;
                }

                Accounts.Add(totalItem);
            }

            CalculateBudgetReport();
        }
    }
}
