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
            List<V110ToV150.OldModels.Transaction> newTransactions = [];

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
                    V110ToV150.OldModels.Transaction newTransaction = new()
                    {
                        Date = oldTransaction.Date,
                        Payee = oldTransaction.Payee,
                        Category = oldTransaction.Category,
                        Amount = oldTransaction.Amount,
                        Memo = oldTransaction.Memo,
                        AccountId = oldAccount.Id,
                        TransactionHash = oldTransaction.TransactionHash,
                        TransactionDetail = oldTransaction.TransactionDetail,
                    };
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
