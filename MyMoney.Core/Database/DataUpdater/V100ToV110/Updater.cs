using System;
using System.Collections.Generic;
using System.Text;
using MyMoney.Core.Database;

namespace MyMoney.DataUpdater.V100ToV110
{
    internal class Updater
    {
        public static void Update(IDatabaseManager databaseManager)
        {
            // Changes from v1.0.0 to v1.1.0 are:
            // - Removed Transactions list from Account model
            // - Stored Transactions separately and linked by AccountId

            // Load the existing Accounts collection
            List<OldModels.Account> oldAccounts = databaseManager.GetCollection<OldModels.Account>("Accounts");

            // Prepare new collections
            List<Core.Models.Account> newAccounts = [];
            List<Core.Models.Transaction> newTransactions = [];

            // Migrate data
            foreach (var oldAccount in oldAccounts)
            {
                Core.Models.Account newAccount = new();
                newAccount.Id = oldAccount.Id;
                newAccount.AccountName = oldAccount.AccountName;
                newAccount.Total = oldAccount.Total;
                newAccounts.Add(newAccount);

                // Migrate Transactions
                foreach (var oldTransaction in oldAccount.Transactions)
                {
                    Core.Models.Transaction newTransaction = new(oldTransaction.Date, oldTransaction.Payee, oldTransaction.Category, oldTransaction.Amount, oldTransaction.Memo);
                    newTransaction.AccountId = oldAccount.Id; // Link to Account
                    newTransaction.TransactionHash = oldTransaction.TransactionHash;
                    newTransaction.TransactionDetail = oldTransaction.TransactionDetail;
                    newTransactions.Add(newTransaction);
                }
            }

            // Write new collections back to the database
            databaseManager.WriteCollection("Accounts", newAccounts);
            databaseManager.WriteCollection("Transactions", newTransactions);

            // Update data version
            Dictionary<string, string> settingsDict = databaseManager.GetSettingsDictionary("ApplicationSettings");
            settingsDict["DataVersion"] = "1.1.0";
            databaseManager.WriteSettingsDictionary("ApplicationSettings", settingsDict);
        }
    }
}
