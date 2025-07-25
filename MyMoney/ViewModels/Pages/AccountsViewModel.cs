using System.Collections.ObjectModel;
using MyMoney.Core.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MyMoney.Views.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Core.Database;
using MyMoney.Services.ContentDialogs;
using MyMoney.Views.Controls;

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

        /// <summary>
        /// Available budget categories for transactions
        /// </summary>
        public ObservableCollection<GroupedComboBox.GroupedComboBoxItem> CategoryNames { get; } = [];

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
        private readonly IDatabaseReader _databaseReader;
        private readonly INewAccountDialogService _newAccountDialogService;
        private readonly ITransferDialogService _transferDialogService;
        private readonly ITransactionDialogService _transactionDialogService;
        private readonly IRenameAccountDialogService _renameAccountDialogService;
        private readonly IMessageBoxService _messageBoxService;

        // Database lock object
        private readonly object _databaseLockObject = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountsViewModel"/> class.
        /// </summary>
        /// <param name="contentDialogService">Service for displaying content dialogs</param>
        /// <param name="databaseReader">Service for reading from the database</param>
        /// <param name="newAccountDialogService">Service for the new account dialog</param>
        /// <param name="transferDialogService">Service for the transfer dialog</param>
        /// <param name="transactionDialogService">Service for the transaction dialog</param>
        /// <param name="renameAccountDialogService">Service for the rename account dialog</param>
        /// <param name="messageBoxService">Service for displaying message boxes</param>
        public AccountsViewModel(
            IContentDialogService contentDialogService,
            IDatabaseReader databaseReader,
            INewAccountDialogService newAccountDialogService,
            ITransferDialogService transferDialogService,
            ITransactionDialogService transactionDialogService,
            IRenameAccountDialogService renameAccountDialogService,
            IMessageBoxService messageBoxService)
        {
            _contentDialogService = contentDialogService ?? throw new ArgumentNullException(nameof(contentDialogService));
            _databaseReader = databaseReader ?? throw new ArgumentNullException(nameof(databaseReader));
            _newAccountDialogService = newAccountDialogService ?? throw new ArgumentNullException(nameof(newAccountDialogService));
            _transferDialogService = transferDialogService ?? throw new ArgumentNullException(nameof(transferDialogService));
            _transactionDialogService = transactionDialogService ?? throw new ArgumentNullException(nameof(transactionDialogService));
            _renameAccountDialogService = renameAccountDialogService ?? throw new ArgumentNullException(nameof(renameAccountDialogService));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));

            LoadAccounts();
            LoadCategoryNames();
        }

        private void LoadAccounts()
        {
            lock (_databaseLockObject)
            {
                var accounts = _databaseReader.GetCollection<Account>("Accounts");
                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                }
            }

            TransactionsEnabled = Accounts.Count > 0;
        }

        private void LoadCategoryNames()
        {
            CategoryNames.Clear();

            BudgetCollection budgetCollection;
            lock (_databaseLockObject)
            {
                budgetCollection = new(_databaseReader);
                if (!budgetCollection.DoesCurrentBudgetExist())
                    return;

                var currentBudget = budgetCollection.GetCurrentBudget();
                AddCategoriesToCollection("Income", currentBudget.BudgetIncomeItems.Select(x => x.Category));
                AddCategoriesToCollection("Savings", currentBudget.BudgetSavingsCategories.Select(x => x.CategoryName));
                
                foreach (var expenseGroup in currentBudget.BudgetExpenseItems)
                {
                    AddCategoriesToCollection(expenseGroup.CategoryName, 
                        expenseGroup.SubItems.Select(x => x.Category));
                }
            }
        }

        private void AddCategoriesToCollection(string group, IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                CategoryNames.Add(new GroupedComboBox.GroupedComboBoxItem
                {
                    Group = group,
                    Item = item
                });
            }
        }

        private void SortTransactions()
        {
            if (SelectedAccount == null) return;

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
                DatabaseWriter.WriteCollection("Accounts", [.. Accounts]);
            }
        }

        private async Task<bool> ValidateTransactionAmount(Currency amount, Account account)
        {
            if (amount.Value < 0 && Math.Abs(amount.Value) > account.Total.Value)
            {
                await _messageBoxService.ShowInfoAsync("Error",
                    "The amount of this transaction is greater than the balance of the selected account.", "OK");
                return false;
            }
            return true;
        }

        private void UpdateAccountBalance(Account account, Transaction? oldTransaction, Transaction? newTransaction = null)
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

        private async Task<bool> ConfirmAccountDeletion()
        {
            var result = await _messageBoxService.ShowAsync(
                "Delete Account?",
                "Are you sure you want to delete the selected Account?\nTHIS CANNOT BE UNDONE!",
                "Yes",
                "No");

            return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
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
        private void UpdateSavingsCategory(Transaction transaction, TransactionOperation operation, Transaction? oldTransaction = null)
        {
            if (transaction.Category.Group != "Savings") return;

            BudgetCollection budgetCollection;
            lock (_databaseLockObject)
            {
                budgetCollection = new(_databaseReader);
                if (!budgetCollection.DoesCurrentBudgetExist()) return;

                var currentBudget = budgetCollection.Budgets[budgetCollection.GetCurrentBudgetIndex()];
                var savingsCategory = currentBudget.BudgetSavingsCategories
                    .FirstOrDefault(x => x.CategoryName == transaction.Category.Name);

                if (savingsCategory == null) return;

                switch (operation)
                {
                    case TransactionOperation.Add:
                        savingsCategory.Transactions.Add(transaction);
                        savingsCategory.CurrentBalance += transaction.Amount;
                        break;

                    case TransactionOperation.Edit when oldTransaction != null:
                        var existingTransaction = savingsCategory.Transactions
                            .FirstOrDefault(x => x.TransactionHash == oldTransaction.TransactionHash);
                        if (existingTransaction != null)
                        {
                            savingsCategory.CurrentBalance -= oldTransaction.Amount;
                            if (transaction.Category.Name == oldTransaction.Category.Name)
                            {
                                savingsCategory.CurrentBalance += transaction.Amount;
                                var index = savingsCategory.Transactions.IndexOf(existingTransaction);
                                savingsCategory.Transactions[index] = transaction;
                            }
                            else
                            {
                                savingsCategory.Transactions.Remove(existingTransaction);
                                // Add to new category if it's still a savings category
                                if (transaction.Category.Group == "Savings")
                                {
                                    UpdateSavingsCategory(transaction, TransactionOperation.Add);
                                }
                            }
                        }
                        break;

                    case TransactionOperation.Delete:
                        var transactionToDelete = savingsCategory.Transactions
                            .FirstOrDefault(x => x.TransactionHash == transaction.TransactionHash);
                        if (transactionToDelete != null)
                        {
                            savingsCategory.CurrentBalance -= transaction.Amount;
                            savingsCategory.Transactions.Remove(transactionToDelete);
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
            Delete
        }

        [RelayCommand]
        private async Task CreateNewAccount()
        {
            var viewModel = new NewAccountDialogViewModel();
            _newAccountDialogService.SetViewModel(viewModel);
            var result = await _newAccountDialogService.ShowDialogAsync(_contentDialogService);
            viewModel = _newAccountDialogService.GetViewModel();

            if (result != ContentDialogResult.Primary) return;

            var newAccount = new Account
            {
                AccountName = viewModel.AccountName,
                Total = viewModel.StartingBalance
            };

            Accounts.Add(newAccount);
            SaveAccountsToDatabase();
            TransactionsEnabled = true;
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

            // make sure there's enough money in the account for this transaction
            if (viewModel.NewTransactionIsExpense && Math.Abs(transaction.Amount.Value) > SelectedAccount?.Total.Value)
            {
                await _messageBoxService.ShowInfoAsync("Error",
                    "The amount of this transaction is greater than the balance of the selected account.", "OK");
                return (false, null);
            }

            return (true, transaction);
        }

        [RelayCommand]
        private async Task CreateNewTransaction()
        {
            if (!EnsureAccountSelected()) return;

            var viewModel = new NewTransactionDialogViewModel
            {
                AutoSuggestPayees = GetAllPayees(),
                SelectedAccountIndex = SelectedAccountIndex,
                Accounts = Accounts,
                SelectedAccount = SelectedAccount,
                CategoryNames = CategoryNames
            };

            var (success, transaction) = await ShowTransactionDialog(viewModel);
            if (!success || transaction == null) return;

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
            if (!ValidateTransactionSelection()) return;

            var oldTransaction = SelectedAccountTransactions[SelectedTransactionIndex];
            var viewModel = CreateTransactionViewModel(oldTransaction);

            var (success, transaction) = await ShowTransactionDialog(viewModel, true);
            if (!success || transaction == null) return;

            UpdateAccountBalance(SelectedAccount!, oldTransaction, transaction);
            SelectedAccountTransactions[SelectedTransactionIndex] = transaction;

            UpdateSavingsCategory(transaction, TransactionOperation.Edit, oldTransaction);

            SortTransactions();
            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task TransferBetweenAccounts()
        {
            if (Accounts.Count < 2) return;

            var accountNames = new ObservableCollection<string>(Accounts.Select(a => a.AccountName));
            var viewModel = new TransferDialogViewModel(accountNames);
            _transferDialogService.SetViewModel(viewModel);
            var result = await _transferDialogService.ShowDialogAsync(_contentDialogService);
            viewModel = _transferDialogService.GetViewModel();

            if (result != ContentDialogResult.Primary) return;

            var fromAccount = Accounts.FirstOrDefault(a => a.AccountName == viewModel.TransferFrom);
            var toAccount = Accounts.FirstOrDefault(a => a.AccountName == viewModel.TransferTo);

            if (fromAccount == null || toAccount == null) return;
            if (!await ValidateTransactionAmount(viewModel.Amount, fromAccount)) return;

            ExecuteTransfer(fromAccount, toAccount, viewModel.Amount);
            SaveAccountsToDatabase();
        }

        private void ExecuteTransfer(Account fromAccount, Account toAccount, Currency amount)
        {
            var fromTransaction = new Transaction(DateTime.Today, 
                "Transfer to " + toAccount.AccountName, 
                new(), 
                new(-amount.Value), 
                "Transfer");

            var toTransaction = new Transaction(DateTime.Today, 
                "Transfer from " + fromAccount.AccountName, 
                new(), 
                amount, 
                "Transfer");

            fromAccount.Transactions.Add(fromTransaction);
            toAccount.Transactions.Add(toTransaction);

            fromAccount.Total += fromTransaction.Amount;
            toAccount.Total += toTransaction.Amount;
        }

        [RelayCommand]
        private async Task DeleteTransaction()
        {
            if (!ValidateTransactionSelection()) return;

            if (!await ConfirmDeletion("Delete Transaction?", 
                "Are you sure you want to delete the selected transaction?")) return;

            var transaction = SelectedAccountTransactions[SelectedTransactionIndex];
            UpdateAccountBalance(SelectedAccount!, transaction);
            UpdateSavingsCategory(transaction, TransactionOperation.Delete);
            
            SelectedAccountTransactions.RemoveAt(SelectedTransactionIndex);
            SaveAccountsToDatabase();
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            if (SelectedAccountIndex < 0) return;

            if (!await ConfirmDeletion("Delete Account?", 
                "Are you sure you want to delete the selected Account?\nTHIS CANNOT BE UNDONE!")) return;

            Accounts.RemoveAt(SelectedAccountIndex);
            UpdateAccountIds();
            SaveAccountsToDatabase();
            TransactionsEnabled = Accounts.Count > 0;
        }

        [RelayCommand]
        private async Task RenameAccount(object content)
        {
            if (SelectedAccountIndex < 0) return;

            var viewModel = new RenameAccountViewModel
            {
                NewName = Accounts[SelectedAccountIndex].AccountName
            };

            _renameAccountDialogService.SetViewModel(viewModel);
            var result = await _renameAccountDialogService.ShowDialogAsync(_contentDialogService);
            viewModel = _renameAccountDialogService.GetViewModel();

            if (result == ContentDialogResult.Primary)
            {
                Accounts[SelectedAccountIndex].AccountName = viewModel.NewName;
                SaveAccountsToDatabase();
            }
        }

        private bool EnsureAccountSelected()
        {
            if (SelectedAccount != null) return true;
            
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
            var result = await _messageBoxService.ShowAsync(
                title, message, "Yes", "No");
            return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
        }

        private NewTransactionDialogViewModel CreateTransactionViewModel(Transaction transaction)
        {
            return new NewTransactionDialogViewModel
            {
                NewTransactionDate = transaction.Date,
                NewTransactionAmount = new(Math.Abs(transaction.Amount.Value)),
                NewTransactionCategorySelectedIndex = GetCategoryIndex(transaction.Category),
                NewTransactionIsExpense = transaction.Amount.Value < 0m,
                NewTransactionIsIncome = transaction.Amount.Value >= 0m,
                NewTransactionMemo = transaction.Memo,
                NewTransactionPayee = transaction.Payee,
                AutoSuggestPayees = GetAllPayees(),
                SelectedAccountIndex = SelectedAccountIndex,
                Accounts = Accounts,
                SelectedAccount = SelectedAccount,
                CategoryNames = CategoryNames,
                AccountsVisibility = Visibility.Collapsed
            };
        }

        private ObservableCollection<string> GetAllPayees()
        {
            return new ObservableCollection<string>(
                Accounts.SelectMany(a => a.Transactions)
                       .Select(t => t.Payee)
                       .Distinct());
        }

        private int GetCategoryIndex(Category category)
        {
            return CategoryNames.TakeWhile(item => 
                !(item.Group == category.Group && item.Item.ToString() == category.Name))
                .Count();
        }

        partial void OnSelectedAccountChanged(Account? value)
        {
            OnPropertyChanged(nameof(SelectedAccountTransactions));
            IsInputEnabled = value != null;
            SortTransactions();
        }

        public void OnPageNavigatedTo()
        {
            LoadCategoryNames();
            TransactionsEnabled = Accounts.Count > 0;
        }
    }
}
