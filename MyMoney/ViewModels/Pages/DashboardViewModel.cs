using LiteDB;
using MyMoney.Models;
using System.Collections.ObjectModel;

namespace MyMoney.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        public ObservableCollection<AccountDashboardDisplayItem> Accounts { get; set; } = [];
        public ObservableCollection<BudgetReportItem> BudgetReportIncomeItems { get; set; } = [];
        public ObservableCollection<BudgetReportItem> BudgetReportExpenseItems { get; set; } = [];

        public DashboardViewModel() 
        {
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
            }
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
            }
        }
    }
}
