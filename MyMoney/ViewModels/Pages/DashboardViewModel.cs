using LiteDB;
using MyMoney.Models;
using System.Collections.ObjectModel;

namespace MyMoney.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        public ObservableCollection<Account> Accounts { get; set; } = [];

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

                    Accounts.Add(account);
                }
            }
        }

    }
}
