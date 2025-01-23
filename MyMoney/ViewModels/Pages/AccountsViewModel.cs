﻿using MyMoney.ViewModels.Windows;
using MyMoney.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyMoney.Core.FS.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using MyMoney.Views.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;


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

        private readonly IContentDialogService _contentDialogService;

        public AccountsViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;

            var a = Core.Database.DatabaseReader.GetCollection<Account>("Accounts");

            foreach (var account in a)
            {
                Accounts.Add(account);
            }

            AddNewAccountButtonClickCommand = new RelayCommand(BttnNewAccount_Click);
            AddTransactionButtonClickCommand = new RelayCommand(BttnNewTransaction_Click);

            LoadCategoryNames();
        }

        private void LoadCategoryNames()
        {
            CategoryNames.Clear();

            Core.Database.BudgetCollection budgetCollection = new();
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

                // Add to the accounts list (shows up in the accounts list view on the accounts page)
                Accounts.Add(newAccount);

                SaveAccountsToDatabase();
            }
        }

        private async void BttnNewTransaction_Click()
        {
            // Make sure that the required fields are filled out
            if (NewTransactionCategory == "")
            {
                // Show message box
                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Missing Category",
                    Content = "Category field cannot be empty",
                    CloseButtonText = "OK"
                };

                await uiMessageBox.ShowDialogAsync();
                return;
            }

            var amount = NewTransactionAmount;
            if (NewTransactionIsExpense) amount = new(-amount.Value);

            // Calculate the balance
            if (SelectedAccount == null) return;
            var balance = SelectedAccount.Total + amount;

            SelectedAccount.Total = balance;

            Transaction newTransaction = new(NewTransactionDate, NewTransactionPayee, NewTransactionCategory, amount, NewTransactionMemo);
            SelectedAccountTransactions.Add(newTransaction);

            NewTransactionDate = DateTime.Today;
            NewTransactionPayee = "";
            NewTransactionCategory = "";
            NewTransactionAmount = new(0m);
            NewTransactionMemo = "";

            SortTransactions();

            SaveAccountsToDatabase();
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
        private void TransferBetweenAccounts()
        {
            ObservableCollection<string> AccountNames = [];

            foreach (var account in Accounts)
            {
                AccountNames.Add(account.AccountName);
            }

            TransferWindowViewModel viewModel = new(AccountNames);

            TransferWindow transferWindow = new(viewModel);

            if (transferWindow.ShowDialog() == true)
            {
                // Transfer the money
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
    }
}
