using MyMoney.ViewModels.Windows;
using MyMoney.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyMoney.Core.FS.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using MyMoney.Views.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using System.Linq;
using MyMoney.Core.Database;

namespace MyMoney.ViewModels.Pages
{
    public partial class AccountsViewModel : ObservableObject
    {
        public ObservableCollection<Account> Accounts { get; set; } = [];
        public ObservableCollection<Transaction> SelectedAccountTransactions => SelectedAccount?.Transactions ?? new ObservableCollection<Transaction>();

        public ObservableCollection<string> CategoryNames { get; set; } = [];

        [ObservableProperty]
        private DateTime _NewTransactionDate = DateTime.Today;

        [ObservableProperty]
        private string _NewTransactionPayee = "";

        [ObservableProperty]
        private string _NewTransactionCategory = "";

        [ObservableProperty]
        private string _NewTransactionMemo = "";

        [ObservableProperty]
        private Currency _NewTransactionAmount = new(0m);

        [ObservableProperty]
        private bool _NewTransactionIsExpense = true;

        [ObservableProperty]
        private bool _NewTransactionIsIncome = false;

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

        [ObservableProperty]
        private bool _TransactionsEnabled = false;

        private readonly IContentDialogService _contentDialogService;
        private readonly IDatabaseReader _databaseReader;

        public ObservableCollection<string> AutoSuggestPayees { get; set; } = [];

        public AccountsViewModel(IContentDialogService contentDialogService, IDatabaseReader databaseReader)
        {
            _contentDialogService = contentDialogService;
            _databaseReader = databaseReader;

            var a = _databaseReader.GetCollection<Account>("Accounts");

            foreach (var account in a)
            {
                Accounts.Add(account);
            }

            if (Accounts.Count > 0) TransactionsEnabled = true;

            LoadCategoryNames();
        }

        private void LoadCategoryNames()
        {
            CategoryNames.Clear();

            BudgetCollection budgetCollection = new(_databaseReader);
            if (!budgetCollection.DoesCurrentBudgetExist())
                return;

            var incomeLst = budgetCollection.GetCurrentBudget().BudgetIncomeItems;
            var expenseLst = budgetCollection.GetCurrentBudget().BudgetExpenseItems;

            foreach (var item in incomeLst)
            {
                CategoryNames.Add(item.Category);
            }

            foreach (var item in expenseLst)
            {
                CategoryNames.Add(item.Category);
            }

        }

        private void SortTransactions()
        {
            var sorted = SelectedAccountTransactions.OrderByDescending(p => p.Date).ToList();
            if (sorted != null)
            {
                SelectedAccountTransactions.Clear();
                foreach (var transaction in sorted)
                {
                    SelectedAccountTransactions.Add(transaction);
                }
            }
        }

        [RelayCommand]
        private async Task BttnNewAccount_Click()
        {
            // Show the new account dialog
            var dialogHost = _contentDialogService.GetDialogHost();
            if (dialogHost == null) return;

            var viewModel = new NewAccountDialogViewModel();

            var newTransactionDialog = new NewAccountDialog(dialogHost, viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
            };

            var result = await newTransactionDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // add the new account
                Account newAccount = new();
                newAccount.AccountName = viewModel.AccountName;
                newAccount.Total = viewModel.StartingBalance;

                // Add to the accounts list (shows up in the accounts list view on the accounts page)
                Accounts.Add(newAccount);

                SaveAccountsToDatabase();

                TransactionsEnabled = true;
            }
        }

        private async Task<(bool success, Transaction? transaction)> ShowTransactionDialog(bool isEdit = false)
        {
            var dialogHost = _contentDialogService.GetDialogHost();
            if (dialogHost == null) return (false, null);

            if (!isEdit)
            {
            // Set default account if it's not already set
            if (Accounts.Count > 0 && SelectedAccountIndex == -1)
                SelectedAccountIndex = 0;
            }

            // Load the list of payees to use in the auto suggest box
            AutoSuggestPayees = GetAllPayees();

            var transactionDialog = new NewTransactionDialog(dialogHost, this)
            {
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
            Title = isEdit ? "Edit Transaction" : "New Transaction"
            };

            var result = await transactionDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
            ClearNewTransactionFields();
            return (false, null);
            }

            var amount = NewTransactionAmount;
            if (NewTransactionIsExpense) amount = new(-amount.Value);

            var transaction = new Transaction(NewTransactionDate, transactionDialog.SelectedPayee, 
            NewTransactionCategory, amount, NewTransactionMemo);

            return (true, transaction);
        }

        [RelayCommand]
        private async Task BttnNewTransaction_Click()
        {
            if (SelectedAccount == null) return;

            var (success, transaction) = await ShowTransactionDialog();
            if (!success || transaction == null) return;

            SelectedAccount.Total += transaction.Amount;
            SelectedAccountTransactions.Add(transaction);
            
            ClearNewTransactionFields();
            SortTransactions();
            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task EditTransaction()
        {
            if (SelectedAccount == null || SelectedTransactionIndex < 0) return;

            // Load current transaction values
            var oldTransaction = SelectedAccountTransactions[SelectedTransactionIndex];
            NewTransactionDate = oldTransaction.Date;
            NewTransactionAmount = new(Math.Abs(oldTransaction.Amount.Value));
            NewTransactionCategory = oldTransaction.Category;
            NewTransactionIsExpense = oldTransaction.Amount.Value < 0m;
            NewTransactionIsIncome = !NewTransactionIsExpense;
            NewTransactionMemo = oldTransaction.Memo;
            NewTransactionPayee = oldTransaction.Payee;

            var (success, transaction) = await ShowTransactionDialog(true);
            if (!success || transaction == null) return;

            SelectedAccount.Total -= oldTransaction.Amount;
            SelectedAccount.Total += transaction.Amount;
            SelectedAccountTransactions.Add(transaction);

            ClearNewTransactionFields();
            SortTransactions();
            SaveAccountsToDatabase();
        }

        private void ClearNewTransactionFields()
        {
            NewTransactionDate = DateTime.Today;
            NewTransactionPayee = "";
            NewTransactionCategory = "";
            NewTransactionAmount = new(0m);
            NewTransactionMemo = "";
        }

        [RelayCommand]
        private void ClearTransaction()
        {
            NewTransactionAmount = new(0m);
            NewTransactionCategory = "";
            NewTransactionDate = DateTime.Today;
            NewTransactionMemo = "";
            NewTransactionPayee = "";
        }

        [RelayCommand]
        private async Task TransferBetweenAccounts()
        {
            ObservableCollection<string> AccountNames = [];

            foreach (var account in Accounts)
            {
                AccountNames.Add(account.AccountName);
            }

            var dialogHost = _contentDialogService.GetDialogHost();
            if (dialogHost == null) return;

            var viewModel = new TransferDialogViewModel(AccountNames);

            var newTransferDialog = new TransferDialog(dialogHost, viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
            };
            var result = await newTransferDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Transfer the money
                Transfer(viewModel);
            }
        }

        public void Transfer(TransferDialogViewModel viewModel)
        {
            // create a new transaction in each of the accounts

            // create FROM transaction
            Transaction FROM = new(DateTime.Today, "Transfer to " + viewModel.TransferTo, "", new(-viewModel.Amount.Value), "Transfer");

            // Create TO transaction
            Transaction TO = new(DateTime.Today, "Transfer TO " + viewModel.TransferTo, "", viewModel.Amount, "Transfer");

            // Add the transactions to their accounts
            for (int i = 0; i < Accounts.Count; i++)
            {
                if (Accounts[i].AccountName == viewModel.TransferFrom)
                {
                    Accounts[i].Transactions.Add(FROM);

                    // Update ending balance
                    Accounts[i].Total += FROM.Amount;
                }
                else if (Accounts[i].AccountName == viewModel.TransferTo)
                {
                    Accounts[i].Transactions.Add(TO);

                    // Update ending balance
                    Accounts[i].Total += TO.Amount;
                }
            }

            SortTransactions();

            // save the accounts to the database
            SaveAccountsToDatabase();
        }

        partial void OnSelectedAccountChanged(Account? value)
        {
            OnPropertyChanged(nameof(SelectedAccountTransactions));

            if (SelectedAccount != null) IsInputEnabled = true;
            else IsInputEnabled = false;

            SortTransactions();
        }

        [ObservableProperty]
        private bool _IsInputEnabled = false;

        private void SaveAccountsToDatabase()
        {
            Core.Database.DatabaseWriter.WriteCollection("Accounts", [.. Accounts]);
        }

        [RelayCommand]
        private async Task DeleteTransaction()
        {
            // Make sure a transaction is selected
            if (SelectedAccount == null) return;
            if (SelectedTransactionIndex < 0) return;

            // confirm deletion
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

            // get the amount of the selected transaction so we can modify the account total
            var amount = SelectedAccountTransactions[SelectedTransactionIndex].Amount;
            SelectedAccount.Total -= amount;

            // Delete the selected transaction
            SelectedAccountTransactions.RemoveAt(SelectedTransactionIndex);

            // Apply changes to database
            SaveAccountsToDatabase();
        }

        public void OnPageNavigatedTo()
        {
            // Reload the categories from the database
            LoadCategoryNames();

            // Check to see if we should enable new transactions
            // Disable new transactions if there are no more accounts
            if (Accounts.Count == 0)
                TransactionsEnabled = false;
            else
                TransactionsEnabled = true;
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

            // Disable new transactions if there are no more accounts
            if (Accounts.Count == 0)
                TransactionsEnabled = false;
            else
                TransactionsEnabled = true;
        }

        [RelayCommand]
        private async Task RenameAccount(object content)
        {
            var dialogHost = _contentDialogService.GetDialogHost();
            if (dialogHost == null) return;

            RenameAccountViewModel renameViewModel = new()
            {
                NewName = Accounts[SelectedAccountIndex].AccountName
            };

            var renameContentDialog = new RenameAccountDialog(dialogHost, renameViewModel)
            {
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
            };
            var result = await renameContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Accounts[SelectedAccountIndex].AccountName = renameViewModel.NewName;
            }

            SaveAccountsToDatabase();
        }

        private ObservableCollection<string> GetAllPayees()
        {
            ObservableCollection<string> payees = [];

            foreach (var account in Accounts)
            {
                foreach (var transaction in from transaction in account.Transactions
                                            where !payees.Contains(transaction.Payee)
                                            select transaction)
                {
                    payees.Add(transaction.Payee);
                }
            }

            return payees;
        }
    }
}
