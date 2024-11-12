using LiteDB;
using MyMoney.Models;
using MyMoney.ViewModels.Windows;
using MyMoney.Views.Windows;
using System.Collections.ObjectModel;
using System.Security.Principal;
using System.Windows.Input;
using Wpf.Ui;


namespace MyMoney.ViewModels.Pages
{
    public partial class AccountsViewModel : ObservableObject
    {
        public ObservableCollection<Account> Accounts { get; set; } = [];
        public ObservableCollection<Transaction> SelectedAccountTransactions => SelectedAccount?.Transactions ?? new ObservableCollection<Transaction>();

        public ObservableCollection<string> CategoryNames { get; set; } = [];

        public ICommand AddNewAccountButtonClickCommand { get; private set; }
        public ICommand AddTransactionButtonClickCommand { get; private set; }

        [ObservableProperty]
        private DateTime _NewTransactionDate = DateTime.Today;

        [ObservableProperty]
        private string _NewTransactionPayee = "";

        [ObservableProperty]
        private string _NewTransactionCategory = "";

        [ObservableProperty]
        private string _NewTransactionMemo = "";

        [ObservableProperty]
        private Currency _NewTransactionSpend = new(0m);

        [ObservableProperty]
        private Currency _NewTransactionReceive = new(0m);

        [ObservableProperty]
        private Account? _SelectedAccount;

        [ObservableProperty]
        private int _SelectedAccountIndex = 0;

        [ObservableProperty]
        private Transaction? _SelectedTransaction;

        [ObservableProperty]
        private int _SelectedTransactionIndex = -1;

        [ObservableProperty]
        private string _AddTransactionButtonText = "Add Transaction";

        public AccountsViewModel()
        {

            // load accounts and their transactions from the database
            using (var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath()))
            {
                // load the accounts list
                var AccountsList = db.GetCollection<Account>("Accounts");

                // iterate over the accounts in the database and add them to the Accounts collection
                for (int i = 1; i <= AccountsList.Count(); i++)
                {
                    var account = AccountsList.FindById(i);

                    // Set the memo and category to an empty string on the first transaction (beginning balance)
                    account.Transactions[0].Memo = "";
                    account.Transactions[0].Category = "";

                    Accounts.Add(account);
                }
            }

            AddNewAccountButtonClickCommand = new RelayCommand(BttnNewAccount_Click);
            AddTransactionButtonClickCommand = new RelayCommand(BttnNewTransaction_Click);

            LoadCategoryNames();
        }

        private void LoadCategoryNames()
        {
            CategoryNames.Clear();

            using var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath());

            // load the category names from the database
            var incomeCollection = db.GetCollection<BudgetIncomeItem>("BudgetIncomeItems");
            var expenseCollection = db.GetCollection<BudgetExpenseItem>("BudgetExpenseItems");

            // load the income items collection
            for (int i = 1; i <= incomeCollection.Count(); i++)
            {
                CategoryNames.Add(incomeCollection.FindById(i).Category);
            }

            // Load the expense items collection
            for (int i = 1; i <= expenseCollection.Count(); i++)
            {
                CategoryNames.Add(expenseCollection.FindById(i).Category);
            }

        }

        private void BttnNewAccount_Click()
        {
            // Show the new account dialog
            NewAccountWindowViewModel newAccountDialogViewModel = new();
            NewAccountWindow newAccountDialog = new(newAccountDialogViewModel);

            if (newAccountDialog.ShowDialog() == true)
            {
                // add the new account
                Account newAccount = new();
                newAccount.AccountName = newAccountDialogViewModel.AccountName;
                newAccount.Total = newAccountDialogViewModel.StartingBalance;

                // add a beginning balance transaction to the account
                newAccount.Transactions.Add(new(DateTime.Today, "Begining Balance", string.Empty, new(0.00m), new(0.0m), newAccountDialogViewModel.StartingBalance, string.Empty));

                // Add to the accounts list (shows up in the accounts list view on the accounts page)
                Accounts.Add(newAccount);

                SaveAccountsToDatabase();
            }
        }

        private void BttnNewTransaction_Click()
        {
            if (AddTransactionButtonText == "Add Transaction")
            {
                // Calculate the balance
                if (SelectedAccount == null) return;
                var balance = SelectedAccount.Total - NewTransactionSpend + NewTransactionReceive;

                SelectedAccount.Total = balance;

                Transaction newTransaction = new(NewTransactionDate, NewTransactionPayee, NewTransactionCategory, new(NewTransactionSpend.Value), new(NewTransactionReceive.Value), new(balance.Value), NewTransactionMemo);
                SelectedAccountTransactions.Add(newTransaction);
            }
            else // editing the selected transaction
            {
                int editTransactionIndex = SelectedTransactionIndex;

                if (SelectedAccount == null || SelectedTransaction == null) return;

                // recalculate the balance by getting the balance for the previous transaction
                if (editTransactionIndex == 0) // first transaction, this shows the begining balance of the account and should not be edited
                    return;

                decimal previousBalance = SelectedAccountTransactions[editTransactionIndex - 1].Balance.Value;
                decimal newBalance = previousBalance - NewTransactionSpend.Value + NewTransactionReceive.Value;

                // Create the transaction object
                Transaction newTransaction = new(NewTransactionDate, NewTransactionPayee, NewTransactionCategory, new(NewTransactionSpend.Value), new(NewTransactionReceive.Value), new(newBalance), NewTransactionMemo);

                // replace the old transaction with the new one
                SelectedAccountTransactions[editTransactionIndex] = newTransaction;

                // Now we have to recalculate the balances for all the transactions after this

                if (SelectedAccountTransactions.Count == editTransactionIndex + 1) // last transaction, no recalculations
                {
                    SelectedAccount.Total = new(newBalance);
                    return;
                }

                for (int i = editTransactionIndex + 1; i < SelectedAccountTransactions.Count; i++)
                {
                    var spend = SelectedAccountTransactions[i].Spend;
                    var receive = SelectedAccountTransactions[i].Receive;

                    SelectedAccountTransactions[i].Balance = SelectedAccountTransactions[i - 1].Balance - spend + receive;
                }

                // update the account total
                SelectedAccount.Total = SelectedAccount.Transactions[SelectedAccount.Transactions.Count - 1].Balance;
            }

            NewTransactionDate = DateTime.Today;
            NewTransactionPayee = "";
            NewTransactionCategory = "";
            NewTransactionSpend = new(0m);
            NewTransactionReceive = new(0m);
            NewTransactionMemo = "";

            SaveAccountsToDatabase();
        }

        partial void OnSelectedAccountChanged(Account? value)
        {
            OnPropertyChanged(nameof(SelectedAccountTransactions));

            if (SelectedAccount != null) IsInputEnabled = true;
            else IsInputEnabled = false;
        }

        partial void OnSelectedTransactionChanged(Transaction? value)
        {
            // load the details about the transaction into the fields
            if (SelectedTransaction == null)
            {
                AddTransactionButtonText = "Add Transaction";
                NewTransactionCategory = "";
                NewTransactionDate = DateTime.Today;
                NewTransactionMemo = "";
                NewTransactionPayee = "";
                NewTransactionReceive = new(0m);
                NewTransactionSpend = new(0m);

                return;
            }

            NewTransactionDate = SelectedTransaction.Date;
            NewTransactionCategory = SelectedTransaction.Category;
            NewTransactionMemo = SelectedTransaction.Memo;
            NewTransactionPayee = SelectedTransaction.Payee;
            NewTransactionReceive = SelectedTransaction.Receive;
            NewTransactionSpend = SelectedTransaction.Spend;

            // change button text to "Edit Transaction"
            AddTransactionButtonText = "Edit Transaction";
        }

        [ObservableProperty]
        private bool _IsInputEnabled = false;

        private void SaveAccountsToDatabase()
        {
            using (var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath()))
            {
                var AccountsList = db.GetCollection<Account>("Accounts");

                // clear the collection
                AccountsList.DeleteAll();

                // add the new items to the database
                foreach (var item in Accounts)
                {
                    AccountsList.Insert(item);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteTransaction()
        {
            // Do not delete the first transaction (opening balance)
            if (SelectedTransactionIndex <= 0) return;

            // show a message box
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Delete Transaction?",
                Content = "Are you sure you want to delete the selected transaction?",
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            var result = await uiMessageBox.ShowDialogAsync();

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary) return; // User clicked no

            // Delete the selected transaction
            SelectedAccountTransactions.RemoveAt(SelectedTransactionIndex);

            // Apply changes to database
            SaveAccountsToDatabase();
        }

        public void OnPageNavigatedTo()
        {
            // Reload the categories from the database
            LoadCategoryNames();
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            if (SelectedAccountIndex < 0) return;

            // Show message box asking user if they really want to delete the account
            // show a message box
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Delete Account?",
                Content = "Are you sure you want to delete the selected Account?\nTHIS CANNOT BE UNDONE!",
                IsPrimaryButtonEnabled = false,
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = "Yes",
                CloseButtonText = "No",
                CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary
            };

            var result = await uiMessageBox.ShowDialogAsync();

            if (result != Wpf.Ui.Controls.MessageBoxResult.Secondary) return; // User clicked no

            Accounts.RemoveAt(SelectedAccountIndex);

            // Reset the IDs on the remaining accounts so they're in consecutive order
            for (int i = 0; i < Accounts.Count; i++)
            {
                Accounts[i].Id = i + 1; // ID starts with 1, and loop counter with 0
            }

            // save changes to database
            SaveAccountsToDatabase();
        }
    }
}
