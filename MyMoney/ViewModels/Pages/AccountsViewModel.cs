using System.Collections.ObjectModel;
using MyMoney.Core.Database;
using MyMoney.Core.Exports;
using MyMoney.Core.Models;
using MyMoney.Core.Services.Accounts;
using MyMoney.Core.Services.Budgets;
using MyMoney.Helpers.DropHandlers;
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

        public TransactionAccountDropHandler TransactionAccountDropHandler { get; }

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

        [ObservableProperty]
        private string _searchQuery = "";

        private CancellationTokenSource? _searchDebounceTokenSource;

        private bool _isLoadingTransactions = false;
        private bool _isSearchActive = false;
        private DateTime? _oldestLoadedDate;
        private int _oldestLoadedId;
        private const int PageSize = 25;

        // Dependencies
        private readonly IContentDialogService _contentDialogService;
        private readonly IDatabaseManager _databaseManager;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IContentDialogFactory _contentDialogFactory;
        private readonly IFileDialogService _fileDialogService;
        private readonly ITransactionCsvExporter _transactionCsvExporter;
        private readonly ITransactionQueryService _transactionQueryService;
        private readonly IAccountTransactionService _accountTransactionService;
        private readonly IAccountValidationService _accountValidationService;
        private readonly ISavingsCategoryTransactionSyncService _savingsCategoryTransactionSyncService;

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
            IContentDialogFactory contentDialogFactory,
            IFileDialogService fileDialogService,
            ITransactionCsvExporter transactionCsvExporter,
            ITransactionQueryService? transactionQueryService = null,
            IAccountTransactionService? accountTransactionService = null,
            IAccountValidationService? accountValidationService = null,
            ISavingsCategoryTransactionSyncService? savingsCategoryTransactionSyncService = null
        )
        {
            _contentDialogService = contentDialogService;
            _databaseManager = databaseManager;
            _messageBoxService = messageBoxService;
            _contentDialogFactory = contentDialogFactory;
            _fileDialogService = fileDialogService;
            _transactionCsvExporter = transactionCsvExporter;
            _transactionQueryService = transactionQueryService ?? new TransactionQueryService(databaseManager);
            _accountTransactionService = accountTransactionService ?? new AccountTransactionService(databaseManager);
            _accountValidationService = accountValidationService ?? new AccountValidationService();
            _savingsCategoryTransactionSyncService =
                savingsCategoryTransactionSyncService ?? new SavingsCategoryTransactionSyncService(databaseManager);
            TransactionAccountDropHandler = new(this);

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
            // Don't load more transactions if search is active
            if (_isSearchActive)
                return;

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
            return await _transactionQueryService.GetTransactionsPage(
                new TransactionPageRequest(accountId, before, beforeId, pageSize)
            );
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

        private async Task<bool> ValidateTransactionAmount(
            Currency amount,
            Account account,
            string accountDescription = "selected"
        )
        {
            var result = _accountValidationService.ValidateTransactionAmount(amount, account);
            if (!result.IsValid)
            {
                await _messageBoxService.ShowInfoAsync(
                    "Error",
                    $"The amount of this transaction is greater than the balance of the {accountDescription} account.",
                    "OK"
                );
                return false;
            }
            return true;
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
            _savingsCategoryTransactionSyncService.ApplyTransactionChangeToCurrentBudget(
                transaction,
                operation switch
                {
                    TransactionOperation.Add => TransactionChangeOperation.Add,
                    TransactionOperation.Edit => TransactionChangeOperation.Edit,
                    TransactionOperation.Delete => TransactionChangeOperation.Delete,
                    _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
                },
                oldTransaction
            );
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

            if (SelectedAccount == null) return (false, null);

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
            transaction.AccountId = SelectedAccount.Id;

            // make sure there's enough money in the account for this transaction
            if (viewModel.NewTransactionIsExpense)
            {
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
                AutoSuggestPayees = await GetAllPayees()
            };

            var categories = await Task.Run(() => viewModel.GetBudgetCategoryNames());
            viewModel.SetCategoryNames(categories);

            var (success, transaction) = await ShowTransactionDialog(viewModel);
            if (!success || transaction == null)
                return;

            _accountTransactionService.ApplyTransactionChange(SelectedAccount!, null, transaction);
            SelectedAccountTransactions.Add(transaction);
            await Task.Run(() => SortTransactions());

            // Write the new transaction to the database
            _databaseManager.Insert("Transactions", transaction);

            UpdateSavingsCategory(transaction, TransactionOperation.Add);

            SaveAccountsToDatabase();
        }

        public async Task<bool> MoveTransactionAsync(
            Transaction transaction,
            Account destinationAccount
        )
        {
            var sourceAccount = Accounts.FirstOrDefault(account => account.Id == transaction.AccountId);
            if (
                sourceAccount == null
                || !Accounts.Contains(destinationAccount)
                || sourceAccount.Id == destinationAccount.Id
            )
            {
                return false;
            }

            if (
                transaction.Amount.Value < 0m
                && !await ValidateTransactionAmount(
                    new Currency(Math.Abs(transaction.Amount.Value)),
                    destinationAccount,
                    "destination"
                )
            )
            {
                return false;
            }

            _accountTransactionService.ApplyTransactionChange(sourceAccount, transaction);
            _accountTransactionService.ApplyTransactionChange(destinationAccount, null, transaction);
            transaction.AccountId = destinationAccount.Id;

            _databaseManager.Update("Transactions", transaction);
            SaveAccountsToDatabase();

            SelectedAccountTransactions.Remove(transaction);
            SelectedTransaction = null;
            SelectedTransactionIndex = -1;

            return true;
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

            _accountTransactionService.ApplyTransactionChange(SelectedAccount!, oldTransaction, transaction);
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
            _accountTransactionService.Transfer(fromAccount, toAccount, amount);

            // Reload transactions if one of the accounts is currently selected
            if (SelectedAccount != null)
            {
                if (SelectedAccount.Id == fromAccount.Id || SelectedAccount.Id == toAccount.Id)
                {
                    await LoadTransactions();
                }
            }

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
            _accountTransactionService.ApplyTransactionChange(SelectedAccount!, transaction);
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

            // Delete all transactions for this account
            var accountId = Accounts[SelectedAccountIndex].Id;
            _databaseManager.DeleteMany<Transaction>("Transactions", x => x.AccountId == accountId);

            Accounts.RemoveAt(SelectedAccountIndex);
            SaveAccountsToDatabase();
            TransactionsEnabled = Accounts.Count > 0;
        }

        [RelayCommand]
        private async Task RenameAccount(Account? account)
        {
            if (account == null) return;

            var viewModel = new RenameAccountViewModel { NewName = account.AccountName };

            var dialog = _contentDialogFactory.Create<RenameAccountDialog>();
            dialog.PrimaryButtonText = "Rename";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                account.AccountName = viewModel.NewName;
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

                var balanceTransaction = _accountTransactionService.CreateBalanceUpdateTransaction(
                    SelectedAccount,
                    newBalance,
                    DateTime.Now
                );
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

        [RelayCommand]
        private async Task ExportTransactions()
        {
            if (SelectedAccount == null && Accounts.Count == 0)
                return;

            var viewModel = new ExportTransactionsDialogViewModel();
            var dialog = _contentDialogFactory.Create<ExportTransactionsDialog>();
            dialog.Title = "Export Transactions";
            dialog.PrimaryButtonText = "Export";
            dialog.CloseButtonText = "Cancel";
            dialog.DataContext = viewModel;
            dialog.DialogHostEx = _contentDialogService.GetDialogHostEx();

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            if (viewModel.ExportCurrentAccount && SelectedAccount == null)
            {
                await _messageBoxService.ShowInfoAsync("Error", "No account is currently selected.", "OK");
                return;
            }

            var outputPath = _fileDialogService.ShowSaveCsvFileDialog($"transactions-{DateTime.Now:yyyy-MM-dd}.csv");
            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            var exportOptions = new TransactionCsvExportOptions
            {
                ExportAllAccounts = viewModel.ExportAllAccounts,
                AccountId = viewModel.ExportCurrentAccount ? SelectedAccount?.Id : null,
                UseDateRange = viewModel.UseDateRange,
                StartDate = viewModel.UseDateRange ? viewModel.StartDate.Date : null,
                EndDate = viewModel.UseDateRange ? viewModel.EndDate.Date : null,
                SelectedFields = viewModel.GetSelectedFields(),
                OutputFilePath = outputPath,
            };

            var exportResult = await _transactionCsvExporter.ExportAsync(exportOptions);
            if (!exportResult.Success)
            {
                await _messageBoxService.ShowInfoAsync(
                    "Export Failed",
                    exportResult.ErrorMessage ?? "Unable to export transactions.",
                    "OK"
                );
                return;
            }

            await _messageBoxService.ShowInfoAsync(
                "Export Complete",
                $"Successfully exported {exportResult.RowCount} transactions.",
                "OK"
            );
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
            // Clear search state before reloading for the new account
            if (_searchDebounceTokenSource != null)
            {
                var oldCts = _searchDebounceTokenSource;
                _searchDebounceTokenSource = null;
                oldCts?.Cancel();
                oldCts?.Dispose();
            }
            SearchQuery = "";
            _isSearchActive = false;

            IsInputEnabled = SelectedAccount != null;
            SelectedAccountTransactions.Clear();
            _oldestLoadedDate = null;
            _oldestLoadedId = 0;
            await LoadTransactions();
        }

        partial void OnSearchQueryChanged(string value)
        {
            // Source updates are debounced (Binding.Delay on the search TextBox) so the
            // view model and binding are not hit on every keypress. Search work still
            // runs on a background thread to avoid blocking the UI.
            var oldCts = _searchDebounceTokenSource;
            _searchDebounceTokenSource = null;
            oldCts?.Cancel();
            oldCts?.Dispose();

            if (string.IsNullOrWhiteSpace(value))
            {
                _ = ClearSearchAndReloadAsync();
                return;
            }

            var cts = new CancellationTokenSource();
            _searchDebounceTokenSource = cts;
            var ct = cts.Token;

            _ = Task.Run(
                async () =>
                {
                    try
                    {
                        await ExecuteSearchAsync(value, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        // Superseded by a newer search or account change — expected.
                    }
                },
                ct
            );
        }

        private async Task ExecuteSearchAsync(string query, CancellationToken ct)
        {
            if (SelectedAccount == null)
                return;

            var results = await _databaseManager.SearchTransactionsAsync(SelectedAccount.Id, query);

            if (ct.IsCancellationRequested)
                return;

            // update with dispatcher
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                SelectedAccountTransactions.Clear();
                foreach (var transaction in results)
                {
                    SelectedAccountTransactions.Add(transaction);
                }
                _isSearchActive = true;
            } );
        }

        private async Task ClearSearchAndReloadAsync()
        {
            SelectedAccountTransactions.Clear();
            _oldestLoadedDate = null;
            _oldestLoadedId = 0;
            _isSearchActive = false;
            await LoadTransactions();
        }

        public void OnPageNavigatedTo()
        {
            TransactionsEnabled = Accounts.Count > 0;
        }
    }
}



