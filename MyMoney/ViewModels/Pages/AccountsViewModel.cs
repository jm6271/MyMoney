using System.Collections.ObjectModel;
using System.ComponentModel;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace MyMoney.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for managing and displaying accounts and their transactions
    /// </summary>
    public partial class AccountsViewModel : ObservableObject, INavigationAware
    {
        /// <summary>
        /// Collection of all accounts in the system
        /// </summary>
        public ObservableCollection<Account> Accounts { get; } = [];

        /// <summary>
        /// Transactions for the currently selected account
        /// </summary>
        public ObservableCollection<Transaction> SelectedAccountTransactions { get; set; } = [];

        // Observable properties
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

        private bool _isLoadingTransactions = false;
        private DateTime? _oldestLoadedDate;
        private int _oldestLoadedId;
        private const int PageSize = 25;

        // Dependencies
        private readonly IContentDialogService _contentDialogService;
        private readonly IDatabaseManager _databaseManager;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IContentDialogFactory _contentDialogFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountsViewModel"/> class.
        /// </summary>
        /// <param name="contentDialogService">Service for displaying content dialogs</param>
        /// <param name="databaseManager">Service for reading from the database</param>
        /// <param name="messageBoxService">Service for displaying message boxes</param>
        /// <param name="contentDialogFactory">Factory for creating content dialogs</param>
        public AccountsViewModel(
            IContentDialogService contentDialogService,
            IDatabaseManager databaseManager,
            IMessageBoxService messageBoxService,
            IContentDialogFactory contentDialogFactory
        )
        {
            _contentDialogService = contentDialogService;
            _databaseManager = databaseManager;
            _messageBoxService = messageBoxService;
            _contentDialogFactory = contentDialogFactory;

            LoadAccounts();
        }

        public async Task OnNavigatedToAsync()
        {
            await Task.Run(() =>
            {
                if (Accounts.Count > 0 && SelectedAccountIndex == -1)
                {
                    SelectedAccountIndex = 0;
                }
            });
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void LoadAccounts()
        {
            var accounts = _databaseManager.GetCollection<Account>("Accounts");
            foreach (var account in accounts)
            {
                Accounts.Add(account);
            }

            TransactionsEnabled = Accounts.Count > 0;
        }

        public async Task LoadTransactions()
        {
            if (_isLoadingTransactions)
                return;
            _isLoadingTransactions = true;

            try
            {
                if (SelectedAccount != null)
                {
                    var page = await GetTransactionsPage(
                        SelectedAccount.Id,
                        _oldestLoadedDate,
                        _oldestLoadedId,
                        PageSize
                    );
                    
                    foreach (var transaction in page)
                    {
                        SelectedAccountTransactions.Add(transaction);
                    }

                    if (page.Count > 0)
                    {
                        _oldestLoadedId = page[page.Count - 1].Id;
                        _oldestLoadedDate = page[page.Count - 1].Date;
                    } 
                }
            }
            finally
            {
                _isLoadingTransactions = false;
            }
        }

        public async Task<IReadOnlyList<Transaction>> GetTransactionsPage(
            int accountId,
            DateTime? before,
            int beforeId,
            int pageSize)
        {
            List<Transaction> transactions = [];

            await _databaseManager.QueryAsync<Transaction>("Transactions", async query =>
            {
                var dbQuery = query
                    .Where(t => t.AccountId == accountId);

                if (before.HasValue)
                {
                    dbQuery = dbQuery.Where(t => t.Date < before.Value ||
                        (t.Date == before.Value && t.Id < beforeId));
                }

                transactions = dbQuery
                    .OrderByDescending(t => t.Date)
                    .Limit(pageSize)
                    .ToList();
            });

            return transactions;
        }

        private void SortTransactions()
        {
            if (SelectedAccount == null)
                return;

            var sorted = SelectedAccountTransactions.OrderByDescending(p => p.Date).ToList();
            SelectedAccountTransactions = new(sorted);
            OnPropertyChanged(nameof(SelectedAccountTransactions));
        }

        private void SaveAccountsToDatabase()
        {
            _databaseManager.WriteCollection("Accounts", [.. Accounts]);
        }

        private async Task<bool> ValidateTransactionAmount(Currency amount, Account account)
        {
            if (amount.Value < 0 || Math.Abs(amount.Value) > account.Total.Value)
            {
                await _messageBoxService.ShowInfoAsync(
                    "Error",
                    "The amount of this transaction is greater than the balance of the selected account.",
                    "OK"
                );
                return false;
            }
            return true;
        }

        private static void UpdateAccountBalance(
            Account account,
            Transaction? oldTransaction,
            Transaction? newTransaction = null
        )
        {
            if (oldTransaction != null)
            {
                account.Total -= oldTransaction.Amount;
            }

            if (newTransaction != null)
            {
                account.Total += newTransaction.Amount;
            }
        }

        /// <summary>
        /// Updates the savings category when a transaction is added, edited, or deleted
        /// </summary>
        private void UpdateSavingsCategory(
            Transaction transaction,
            TransactionOperation operation,
            Transaction? oldTransaction = null
        )
        {
            if (
                transaction.Category.Group != "Savings"
                && (operation == TransactionOperation.Add || operation == TransactionOperation.Delete)
            )
                return;

            BudgetCollection budgetCollection;
            budgetCollection = new(_databaseManager);
            if (!budgetCollection.DoesCurrentBudgetExist())
                return;

            var currentBudget = budgetCollection.Budgets[budgetCollection.GetCurrentBudgetIndex()];

            switch (operation)
            {
                case TransactionOperation.Add:
                    var savingsCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                        x.CategoryName == transaction.Category.Name
                    );

                    if (savingsCategory == null)
                        break;

                    savingsCategory.Transactions.Add(transaction);
                    savingsCategory.CurrentBalance += transaction.Amount;
                    break;

                case TransactionOperation.Edit when oldTransaction != null:
                    if (transaction.Category.Name == oldTransaction.Category.Name) // Same category
                    {
                        if (transaction.Category.Group != "Savings")
                            break;

                        var categoryToUpdate = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                            x.CategoryName == transaction.Category.Name
                        );

                        if (categoryToUpdate == null)
                            break;

                        var transactionToUpdate = categoryToUpdate.Transactions.FirstOrDefault(x =>
                            x.TransactionHash == oldTransaction.TransactionHash
                        );
                        if (transactionToUpdate == null)
                            break;

                        categoryToUpdate.CurrentBalance -= oldTransaction.Amount;
                        categoryToUpdate.CurrentBalance += transaction.Amount;

                        var index = categoryToUpdate.Transactions.IndexOf(transactionToUpdate);
                        categoryToUpdate.Transactions[index] = transaction;
                    }
                    else // Category was changed
                    {
                        var originalCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                            x.CategoryName == oldTransaction.Category.Name
                        );
                        if (originalCategory == null)
                            break;

                        var transactionInOldCategory = originalCategory.Transactions.FirstOrDefault(x =>
                            x.TransactionHash == oldTransaction.TransactionHash
                        );
                        if (transactionInOldCategory == null)
                            break;

                        // Remove the transaction from the original savings category
                        originalCategory.Transactions.Remove(transactionInOldCategory);
                        originalCategory.CurrentBalance -= oldTransaction.Amount;

                        // Add the new transaction to the other category if it is a savings category
                        if (transaction.Category.Group == "Savings")
                        {
                            var newCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                                x.CategoryName == transaction.Category.Name
                            );

                            if (newCategory == null)
                                break;

                            newCategory.Transactions.Add(transaction);
                            newCategory.CurrentBalance += transaction.Amount;
                        }
                    }
                    break;

                case TransactionOperation.Delete:
                    var containingCategory = currentBudget.BudgetSavingsCategories.FirstOrDefault(x =>
                        x.CategoryName == transaction.Category.Name
                    );

                    if (containingCategory == null)
                        break;

                    var transactionToDelete = containingCategory.Transactions.FirstOrDefault(x =>
                        x.TransactionHash == transaction.TransactionHash
                    );
                    if (transactionToDelete != null)
                    {
                        containingCategory.CurrentBalance -= transaction.Amount;
                        containingCategory.Transactions.Remove(transactionToDelete);
                    }
                    break;
            }

            budgetCollection.SaveBudgetCollection();
        }

        private enum TransactionOperation
        {
            Add,
            Edit,
            Delete,
        }

        [RelayCommand]
        private async Task CreateNewAccount()
        {
            var viewModel = new NewAccountDialogViewModel();

            var dialog = _contentDialogFactory.Create<NewAccountDialog>();
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            var newAccount = new Account { AccountName = viewModel.AccountName, Total = viewModel.StartingBalance };

            Accounts.Add(newAccount);
            SaveAccountsToDatabase();
            TransactionsEnabled = true;
        }

        private async Task<(bool success, Transaction? transaction)> ShowTransactionDialog(
            NewTransactionDialogViewModel viewModel,
            bool isEdit = false
        )
        {
            if (!isEdit && Accounts.Count > 0 && SelectedAccountIndex == -1)
            {
                SelectedAccountIndex = 0;
            }

            var dialog = _contentDialogFactory.Create<NewTransactionDialog>();
            dialog.Title = isEdit ? "Edit Transaction" : "New Transaction";
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            // If dialog was not confirmed or the service returned null, treat as cancel
            if (result != ContentDialogResult.Primary)
            {
                return (false, null);
            }

            var amount = viewModel.NewTransactionAmount;
            if (viewModel.NewTransactionIsExpense)
            {
                amount = new Currency(-amount.Value);
            }

            var transaction = new Transaction(
                viewModel.NewTransactionDate,
                viewModel.NewTransactionPayee,
                viewModel.NewTransactionCategory,
                amount,
                viewModel.NewTransactionMemo
            );
            transaction.AccountId = viewModel.SelectedAccount!.Id;

            // make sure there's enough money in the account for this transaction
            if (viewModel.NewTransactionIsExpense)
            {
                if (SelectedAccount == null)
                {
                    await _messageBoxService.ShowInfoAsync("Error", "No account is selected.", "OK");
                    return (false, null);
                }

                if (Math.Abs(transaction.Amount.Value) > SelectedAccount.Total.Value)
                {
                    await _messageBoxService.ShowInfoAsync(
                        "Error",
                        "The amount of this transaction is greater than the balance of the selected account.",
                        "OK"
                    );
                    return (false, null);
                }
            }

            return (true, transaction);
        }

        [RelayCommand]
        private async Task CreateNewTransaction()
        {
            if (!EnsureAccountSelected())
                return;

            var viewModel = new NewTransactionDialogViewModel(_databaseManager)
            {
                SelectedAccountIndex = SelectedAccountIndex,
                Accounts = Accounts,
                SelectedAccount = SelectedAccount,
                AutoSuggestPayees = await GetAllPayees()
            };

            var categories = await Task.Run(() => viewModel.GetBudgetCategoryNames());
            viewModel.SetCategoryNames(categories);

            var (success, transaction) = await ShowTransactionDialog(viewModel);
            if (!success || transaction == null)
                return;

            SelectedAccountIndex = viewModel.SelectedAccountIndex;
            UpdateAccountBalance(SelectedAccount!, null, transaction);
            SelectedAccountTransactions.Add(transaction);
            await Task.Run(() => SortTransactions());

            // Write the new transaction to the database
            _databaseManager.Insert("Transactions", transaction);

            UpdateSavingsCategory(transaction, TransactionOperation.Add);

            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task EditTransaction()
        {
            if (!ValidateTransactionSelection())
                return;

            var oldTransaction = SelectedAccountTransactions[SelectedTransactionIndex];
            var viewModel = await CreateTransactionViewModel(oldTransaction);

            var categories = await Task.Run(() => viewModel.GetBudgetCategoryNames());
            viewModel.SetCategoryNames(categories);

            viewModel.SetSelectedCategoryByName(oldTransaction.Category.Name);

            var (success, transaction) = await ShowTransactionDialog(viewModel, true);
            if (!success || transaction == null)
                return;
            transaction.Id = oldTransaction.Id;

            UpdateAccountBalance(SelectedAccount!, oldTransaction, transaction);
            SelectedAccountTransactions[SelectedTransactionIndex] = transaction;
            await Task.Run(() => SortTransactions());

            // Update the transaction in the database
            _databaseManager.Update("Transactions", transaction);

            UpdateSavingsCategory(transaction, TransactionOperation.Edit, oldTransaction);

            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task TransferBetweenAccounts()
        {
            if (Accounts.Count < 2)
                return;

            var accountNames = new ObservableCollection<string>(Accounts.Select(a => a.AccountName));
            var viewModel = new TransferDialogViewModel(accountNames);

            var dialog = _contentDialogFactory.Create<TransferDialog>();
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            var fromAccount = Accounts.FirstOrDefault(a => a.AccountName == viewModel.TransferFrom);
            var toAccount = Accounts.FirstOrDefault(a => a.AccountName == viewModel.TransferTo);

            if (fromAccount == null || toAccount == null)
                return;
            if (!await ValidateTransactionAmount(viewModel.Amount, fromAccount))
                return;

            await ExecuteTransfer(fromAccount, toAccount, viewModel.Amount);
            SaveAccountsToDatabase();
        }

        private async Task ExecuteTransfer(Account fromAccount, Account toAccount, Currency amount)
        {
            var fromTransaction = new Transaction(
                DateTime.Today,
                "Transfer to " + toAccount.AccountName,
                new(),
                new(-amount.Value),
                "Transfer"
            );
            fromTransaction.AccountId = fromAccount.Id;

            var toTransaction = new Transaction(
                DateTime.Today,
                "Transfer from " + fromAccount.AccountName,
                new(),
                amount,
                "Transfer"
            );
            toTransaction.AccountId = toAccount.Id;

            // Write the new transactions to the database
            _databaseManager.Insert("Transactions", fromTransaction);
            _databaseManager.Insert("Transactions", toTransaction);

            // Reload transactions if one of the accounts is currently selected
            if (SelectedAccount != null)
            {
                if (SelectedAccount.Id == fromAccount.Id || SelectedAccount.Id == toAccount.Id)
                {
                    await LoadTransactions();
                }
            }

            fromAccount.Total += fromTransaction.Amount;
            toAccount.Total += toTransaction.Amount;

            // Reload the transactions in the selected account to ensure they are updated
            var accountIndex = SelectedAccountIndex;
            SelectedAccountIndex = -1;
            SelectedAccountIndex = accountIndex;
        }

        [RelayCommand]
        private async Task DeleteTransaction()
        {
            if (!ValidateTransactionSelection())
                return;

            if (
                !await ConfirmDeletion(
                    "Delete Transaction?",
                    "Are you sure you want to delete the selected transaction?"
                )
            )
                return;

            var transaction = SelectedAccountTransactions[SelectedTransactionIndex];
            UpdateAccountBalance(SelectedAccount!, transaction);
            UpdateSavingsCategory(transaction, TransactionOperation.Delete);

            SelectedAccountTransactions.RemoveAt(SelectedTransactionIndex);

            _databaseManager.Delete<Transaction>("Transactions", transaction.Id);

            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            if (SelectedAccountIndex < 0)
                return;

            if (
                !await ConfirmDeletion(
                    "Delete Account?",
                    "Are you sure you want to delete the selected Account?\nTHIS CANNOT BE UNDONE!"
                )
            )
                return;

            Accounts.RemoveAt(SelectedAccountIndex);
            SaveAccountsToDatabase();
            TransactionsEnabled = Accounts.Count > 0;
        }

        [RelayCommand]
        private async Task RenameAccount()
        {
            if (SelectedAccountIndex < 0)
                return;

            var viewModel = new RenameAccountViewModel { NewName = Accounts[SelectedAccountIndex].AccountName };

            var dialog = _contentDialogFactory.Create<RenameAccountDialog>();
            dialog.PrimaryButtonText = "Rename";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Accounts[SelectedAccountIndex].AccountName = viewModel.NewName;
                SaveAccountsToDatabase();
            }
        }

        [RelayCommand]
        private async Task UpdateAccountBalance()
        {
            if (SelectedAccountIndex < 0 || SelectedAccount == null)
                return;

            var viewModel = new UpdateAccountBalanceDialogViewModel { Balance = SelectedAccount.Total };

            var dialog = _contentDialogFactory.Create<UpdateAccountBalanceDialog>();
            dialog.PrimaryButtonText = "Update";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newBalance = viewModel.Balance;

                // Create a transaction to reflect the balance change
                var balanceChange = newBalance - SelectedAccount.Total;
                var balanceTransaction = new Transaction(
                    DateTime.Now,
                    "Balance update",
                    new(),
                    balanceChange,
                    "Balance update"
                );
                balanceTransaction.AccountId = SelectedAccount.Id;
                SelectedAccountTransactions.Add(balanceTransaction);
                SortTransactions();

                _databaseManager.Insert("Transactions", balanceTransaction);

                SelectedAccount.Total = newBalance;

                SaveAccountsToDatabase();
            }
        }

        [RelayCommand]
        private async Task ReconcileTransaction(Transaction transaction)
        {
            _databaseManager.Update("Transactions", transaction);
        }

        private bool EnsureAccountSelected()
        {
            if (SelectedAccount != null)
                return true;

            if (Accounts.Count > 0)
            {
                SelectedAccountIndex = 0;
                SelectedAccount = Accounts[SelectedAccountIndex];
                return true;
            }

            return false;
        }

        private bool ValidateTransactionSelection()
        {
            return SelectedAccount != null && SelectedTransactionIndex >= 0;
        }

        private async Task<bool> ConfirmDeletion(string title, string message)
        {
            var result = await _messageBoxService.ShowAsync(title, message, "Yes", "No");
            return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
        }

        private async Task<NewTransactionDialogViewModel> CreateTransactionViewModel(Transaction transaction)
        {
            return new NewTransactionDialogViewModel(_databaseManager)
            {
                NewTransactionDate = transaction.Date,
                NewTransactionAmount = new(Math.Abs(transaction.Amount.Value)),
                NewTransactionIsExpense = transaction.Amount.Value < 0m,
                NewTransactionIsIncome = transaction.Amount.Value >= 0m,
                NewTransactionMemo = transaction.Memo,
                NewTransactionPayee = transaction.Payee,
                AutoSuggestPayees = await GetAllPayees(),
                SelectedAccountIndex = SelectedAccountIndex,
                Accounts = Accounts,
                SelectedAccount = SelectedAccount,
                AccountsVisibility = Visibility.Collapsed,
            };
        }

        private async Task<List<string>> GetAllPayees()
        {
            var transactions = new List<string>();
            await _databaseManager.QueryAsync<Transaction>("Transactions", async query =>
            {
                transactions = [.. query
                    .Where(t => t.Payee != null)
                    .Select(t => t.Payee)
                    .ToList()
                    .Distinct()];
            });
            return transactions;
        }

        public async Task SelectedAccountChanged()
        {
            IsInputEnabled = SelectedAccount != null;
            SelectedAccountTransactions.Clear();
            _oldestLoadedDate = null;
            _oldestLoadedId = 0;
            await LoadTransactions();
        }

        public void OnPageNavigatedTo()
        {
            TransactionsEnabled = Accounts.Count > 0;
        }
    }
}
