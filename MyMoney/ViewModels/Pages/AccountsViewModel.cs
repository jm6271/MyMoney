using System.Collections.ObjectModel;
using System.ComponentModel;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for managing and displaying accounts and their transactions
    /// </summary>
    public partial class AccountsViewModel : ObservableObject
    {
        /// <summary>
        /// Collection of all accounts in the system
        /// </summary>
        public ObservableCollection<Account> Accounts { get; } = [];

        /// <summary>
        /// Transactions for the currently selected account
        /// </summary>
        public ObservableCollection<Transaction> SelectedAccountTransactions =>
            SelectedAccount?.Transactions ?? new ObservableCollection<Transaction>();

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

        // Dependencies
        private readonly IContentDialogService _contentDialogService;
        private readonly IDatabaseManager _databaseManager;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IContentDialogFactory _contentDialogFactory;

        // Database lock object
        private readonly object _databaseLockObject = new();

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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(SelectedAccount))
            {
                OnPropertyChanged(nameof(SelectedAccountTransactions));
            }
        }

        private void LoadAccounts()
        {
            lock (_databaseLockObject)
            {
                var accounts = _databaseManager.GetCollection<Account>("Accounts");
                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                }
            }

            TransactionsEnabled = Accounts.Count > 0;
        }

        private void SortTransactions()
        {
            if (SelectedAccount == null)
                return;

            var sorted = SelectedAccountTransactions.OrderByDescending(p => p.Date).ToList();
            SelectedAccountTransactions.Clear();
            foreach (var transaction in sorted)
            {
                SelectedAccountTransactions.Add(transaction);
            }
        }

        private void SaveAccountsToDatabase()
        {
            lock (_databaseLockObject)
            {
                _databaseManager.WriteCollection("Accounts", [.. Accounts]);
            }
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

        private void UpdateAccountIds()
        {
            for (var i = 0; i < Accounts.Count; i++)
            {
                Accounts[i].Id = i + 1;
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
            lock (_databaseLockObject)
            {
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
                AutoSuggestPayees = GetAllPayees(),
                SelectedAccountIndex = SelectedAccountIndex,
                Accounts = Accounts,
                SelectedAccount = SelectedAccount,
            };

            var (success, transaction) = await ShowTransactionDialog(viewModel);
            if (!success || transaction == null)
                return;

            SelectedAccountIndex = viewModel.SelectedAccountIndex;
            UpdateAccountBalance(SelectedAccount!, null, transaction);
            SelectedAccountTransactions.Add(transaction);

            UpdateSavingsCategory(transaction, TransactionOperation.Add);

            SortTransactions();
            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task EditTransaction()
        {
            if (!ValidateTransactionSelection())
                return;

            var oldTransaction = SelectedAccountTransactions[SelectedTransactionIndex];
            var viewModel = CreateTransactionViewModel(oldTransaction);
            viewModel.SetSelectedCategoryByName(oldTransaction.Category.Name);

            var (success, transaction) = await ShowTransactionDialog(viewModel, true);
            if (!success || transaction == null)
                return;

            UpdateAccountBalance(SelectedAccount!, oldTransaction, transaction);
            SelectedAccountTransactions[SelectedTransactionIndex] = transaction;

            UpdateSavingsCategory(transaction, TransactionOperation.Edit, oldTransaction);

            SortTransactions();
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

            ExecuteTransfer(fromAccount, toAccount, viewModel.Amount);
            SaveAccountsToDatabase();

            OnPropertyChanged(nameof(SelectedAccountTransactions));
        }

        private void ExecuteTransfer(Account fromAccount, Account toAccount, Currency amount)
        {
            var fromTransaction = new Transaction(
                DateTime.Today,
                "Transfer to " + toAccount.AccountName,
                new(),
                new(-amount.Value),
                "Transfer"
            );

            var toTransaction = new Transaction(
                DateTime.Today,
                "Transfer from " + fromAccount.AccountName,
                new(),
                amount,
                "Transfer"
            );

            fromAccount.Transactions.Add(fromTransaction);
            toAccount.Transactions.Add(toTransaction);

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
            UpdateAccountIds();
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
                SelectedAccount.Transactions.Add(balanceTransaction);

                SelectedAccount.Total = newBalance;

                SortTransactions();
                SaveAccountsToDatabase();
            }
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

        private NewTransactionDialogViewModel CreateTransactionViewModel(Transaction transaction)
        {
            return new NewTransactionDialogViewModel(_databaseManager)
            {
                NewTransactionDate = transaction.Date,
                NewTransactionAmount = new(Math.Abs(transaction.Amount.Value)),
                NewTransactionIsExpense = transaction.Amount.Value < 0m,
                NewTransactionIsIncome = transaction.Amount.Value >= 0m,
                NewTransactionMemo = transaction.Memo,
                NewTransactionPayee = transaction.Payee,
                AutoSuggestPayees = GetAllPayees(),
                SelectedAccountIndex = SelectedAccountIndex,
                Accounts = Accounts,
                SelectedAccount = SelectedAccount,
                AccountsVisibility = Visibility.Collapsed,
            };
        }

        private List<string> GetAllPayees()
        {
            return [.. Accounts.SelectMany(a => a.Transactions).Select(t => t.Payee).Where(p => p != null).Distinct()];
        }

        partial void OnSelectedAccountChanged(Account? value)
        {
            OnPropertyChanged(nameof(SelectedAccountTransactions));
            IsInputEnabled = value != null;
            SortTransactions();
        }

        public void OnPageNavigatedTo()
        {
            TransactionsEnabled = Accounts.Count > 0;
        }
    }
}
