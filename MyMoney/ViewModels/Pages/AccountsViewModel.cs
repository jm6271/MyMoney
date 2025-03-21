using System.Collections.ObjectModel;
using MyMoney.Core.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MyMoney.Views.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Core.Database;
using MyMoney.Services.ContentDialogs;

namespace MyMoney.ViewModels.Pages
{
    public partial class AccountsViewModel : ObservableObject
    {
        public ObservableCollection<Account> Accounts { get; } = [];
        public ObservableCollection<Transaction> SelectedAccountTransactions => SelectedAccount?.Transactions ?? new ObservableCollection<Transaction>();

        public ObservableCollection<string> CategoryNames { get; } = [];

        [ObservableProperty]
        private Account? _selectedAccount;

        [ObservableProperty]
        private int _selectedAccountIndex;

        [ObservableProperty]
        private Transaction? _selectedTransaction;

        [ObservableProperty]
        private int _selectedTransactionIndex = -1;

        [ObservableProperty]
        private string _addTransactionButtonText = "Add Transaction";

        [ObservableProperty]
        private bool _transactionsEnabled;
        
        [ObservableProperty]
        private bool _isInputEnabled;

        private readonly IContentDialogService _contentDialogService;
        private readonly IDatabaseReader _databaseReader;
        private readonly INewAccountDialogService _newAccountDialogService;
        private readonly ITransferDialogService _transferDialogService;
        private readonly ITransactionDialogService _transactionDialogService;
        private readonly IRenameAccountDialogService _renameAccountDialogService;
        private readonly IMessageBoxService _messageBoxService;

        public AccountsViewModel(IContentDialogService contentDialogService, IDatabaseReader databaseReader, 
            INewAccountDialogService newAccountDialogService, ITransferDialogService transferDialogService,
            ITransactionDialogService transactionDialogService, IRenameAccountDialogService renameAccountDialogService,
            IMessageBoxService messageBoxService)
        {
            _contentDialogService = contentDialogService;
            _databaseReader = databaseReader;
            _newAccountDialogService = newAccountDialogService;
            _transferDialogService = transferDialogService;
            _transactionDialogService = transactionDialogService;
            _renameAccountDialogService = renameAccountDialogService;
            _messageBoxService = messageBoxService;

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
            SelectedAccountTransactions.Clear();
            foreach (var transaction in sorted)
            {
                SelectedAccountTransactions.Add(transaction);
            }
        }

        [RelayCommand]
        private async Task CreateNewAccount()
        {
            var viewModel = new NewAccountDialogViewModel();
            _newAccountDialogService.SetViewModel(viewModel);
            var result = await _newAccountDialogService.ShowDialogAsync(_contentDialogService);
            viewModel = _newAccountDialogService.GetViewModel();

            if (result == ContentDialogResult.Primary)
            {
                // add the new account
                Account newAccount = new()
                {
                    AccountName = viewModel.AccountName,
                    Total = viewModel.StartingBalance
                };

                // Add to the accounts list (shows up in the accounts list view on the accounts page)
                Accounts.Add(newAccount);

                SaveAccountsToDatabase();

                TransactionsEnabled = true;
            }
        }

        private async Task<(bool success, Transaction? transaction)> ShowTransactionDialog(NewTransactionDialogViewModel viewModel, bool isEdit = false)
        {
            if (!isEdit && Accounts.Count > 0 && SelectedAccountIndex == -1)
            {
                SelectedAccountIndex = 0;
            }

            _transactionDialogService.SetViewModel(viewModel);
            _transactionDialogService.SetTitle(isEdit ? "Edit Transaction" : "New Transaction");
            var result = await _transactionDialogService.ShowDialogAsync(_contentDialogService);
            viewModel = _transactionDialogService.GetViewModel();

            if (result != ContentDialogResult.Primary)
            {
                return (false, null);
            }

            var amount = viewModel.NewTransactionAmount;
            if (viewModel.NewTransactionIsExpense) amount = new(-amount.Value);

            var transaction = new Transaction(viewModel.NewTransactionDate, _transactionDialogService.GetSelectedPayee(), 
            viewModel.NewTransactionCategory, amount, viewModel.NewTransactionMemo);

            return (true, transaction);
        }

        [RelayCommand]
        private async Task CreateNewTransaction()
        {
            if (SelectedAccount == null)
            {
                if (Accounts.Count > 0)
                {
                    SelectedAccountIndex = 0;
                    SelectedAccount = Accounts[SelectedAccountIndex];
                }
                else
                {
                    return;
                }
            }

            NewTransactionDialogViewModel viewModel = new();
            viewModel.AutoSuggestPayees = GetAllPayees();
            viewModel.SelectedAccountIndex = SelectedAccountIndex;
            viewModel.Accounts = Accounts;
            viewModel.SelectedAccount = SelectedAccount;
            viewModel.CategoryNames = CategoryNames;

            var (success, transaction) = await ShowTransactionDialog(viewModel);
            if (!success || transaction == null) return;

            SelectedAccountIndex = viewModel.SelectedAccountIndex;
            SelectedAccount.Total += transaction.Amount;
            SelectedAccountTransactions.Add(transaction);
            
            SortTransactions();
            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task EditTransaction()
        {
            if (SelectedAccount == null || SelectedTransactionIndex < 0) return;

            // Load current transaction values
            NewTransactionDialogViewModel viewModel = new();
            var oldTransaction = SelectedAccountTransactions[SelectedTransactionIndex];
            viewModel.NewTransactionDate = oldTransaction.Date;
            viewModel.NewTransactionAmount = new(Math.Abs(oldTransaction.Amount.Value));
            viewModel.NewTransactionCategory = oldTransaction.Category;
            viewModel.NewTransactionIsExpense = oldTransaction.Amount.Value < 0m;
            viewModel.NewTransactionIsIncome = !viewModel.NewTransactionIsExpense;
            viewModel.NewTransactionMemo = oldTransaction.Memo;
            viewModel.NewTransactionPayee = oldTransaction.Payee;
            viewModel.AutoSuggestPayees = GetAllPayees();
            viewModel.SelectedAccountIndex = SelectedAccountIndex;
            viewModel.Accounts = Accounts;
            viewModel.SelectedAccount = SelectedAccount;
            viewModel.CategoryNames = CategoryNames;
            viewModel.AccountsVisibility = Visibility.Collapsed;

            var (success, transaction) = await ShowTransactionDialog(viewModel, true);
            if (!success || transaction == null) return;

            SelectedAccount.Total -= oldTransaction.Amount;
            SelectedAccount.Total += transaction.Amount;
            SelectedAccountTransactions[SelectedTransactionIndex] = transaction;

            SortTransactions();
            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task TransferBetweenAccounts()
        {
            ObservableCollection<string> accountNames = [];

            foreach (var account in Accounts)
            {
                accountNames.Add(account.AccountName);
            }

            var viewModel = new TransferDialogViewModel(accountNames);
            _transferDialogService.SetViewModel(viewModel);
            var result = await _transferDialogService.ShowDialogAsync(_contentDialogService);
            viewModel = _transferDialogService.GetViewModel();

            if (result == ContentDialogResult.Primary)
            {
                // Transfer the money
                // create a new transaction in each of the accounts

                // create FROM transaction
                Transaction from = new(DateTime.Today, "Transfer to " + viewModel.TransferTo, "", new(-viewModel.Amount.Value), "Transfer");

                // Create TO transaction
                Transaction to = new(DateTime.Today, "Transfer TO " + viewModel.TransferTo, "", viewModel.Amount, "Transfer");

                // Add the transactions to their accounts
                foreach (var t in Accounts)
                {
                    if (t.AccountName == viewModel.TransferFrom)
                    {
                        t.Transactions.Add(from);

                        // Update ending balance
                        t.Total += from.Amount;
                    }
                    else if (t.AccountName == viewModel.TransferTo)
                    {
                        t.Transactions.Add(to);

                        // Update ending balance
                        t.Total += to.Amount;
                    }
                }
            }
        }

        partial void OnSelectedAccountChanged(Account? value)
        {
            OnPropertyChanged(nameof(SelectedAccountTransactions));

            IsInputEnabled = SelectedAccount != null;

            SortTransactions();
        }

        private void SaveAccountsToDatabase()
        {
            DatabaseWriter.WriteCollection("Accounts", [.. Accounts]);
        }

        [RelayCommand]
        private async Task DeleteTransaction()
        {
            // Make sure a transaction is selected
            if (SelectedAccount == null) return;
            if (SelectedTransactionIndex < 0) return;
            
            var result = await _messageBoxService.ShowAsync(
                "Delete Transaction?",
                "Are you sure you want to delete the selected transaction?",
                "Yes",
                "No");

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
            TransactionsEnabled = Accounts.Count != 0;
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            if (SelectedAccountIndex < 0) return;

            // Show message box asking user if they really want to delete the account
            // show a message box
            var result = await _messageBoxService.ShowAsync(
                "Delete Account?",
                "Are you sure you want to delete the selected Account?\nTHIS CANNOT BE UNDONE!",
                "Yes",
                "No");

            if (result != Wpf.Ui.Controls.MessageBoxResult.Primary) return; // User clicked no

            Accounts.RemoveAt(SelectedAccountIndex);

            // Reset the IDs on the remaining accounts so they're in consecutive order
            for (var i = 0; i < Accounts.Count; i++)
            {
                Accounts[i].Id = i + 1; // ID starts with 1, and loop counter with 0
            }

            // save changes to database
            SaveAccountsToDatabase();

            // Disable new transactions if there are no more accounts
            TransactionsEnabled = Accounts.Count != 0;
        }

        [RelayCommand]
        private async Task RenameAccount(object content)
        {
            RenameAccountViewModel renameViewModel = new()
            {
                NewName = Accounts[SelectedAccountIndex].AccountName
            };

            _renameAccountDialogService.SetViewModel(renameViewModel);
            var result = await _renameAccountDialogService.ShowDialogAsync(_contentDialogService);
            renameViewModel = _renameAccountDialogService.GetViewModel();

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
