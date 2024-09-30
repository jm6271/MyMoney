using MyMoney.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wpf.Ui;


namespace MyMoney.ViewModels.Pages
{
    public partial class AccountsViewModel : ObservableObject
    {
        public ObservableCollection<Account> Accounts { get; set; } = [];
        public ObservableCollection<Transaction> SelectedAccountTransactions => SelectedAccount?.Transactions ?? new ObservableCollection<Transaction>();

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
        private IContentDialogService _contentDialogService;

        public AccountsViewModel(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;

            Models.Account account = new()
            {
                AccountName = "Savings",
                Total = new(500)
            };
            account.Transactions.Add(new(DateTime.Today, "Begining Balance", "", new(0.00m), new(0.0m), new(500m), ""));
            Accounts.Add(account);

            AddNewAccountButtonClickCommand = new RelayCommand(BttnNewAccount_Click);
            AddTransactionButtonClickCommand = new RelayCommand(BttnNewTransaction_Click);
        }


        private void BttnNewAccount_Click()
        {

        }

        private void BttnNewTransaction_Click()
        {
            // Calculate the balance
            if (SelectedAccount == null) return;
            var balance = SelectedAccount.Total - NewTransactionSpend + NewTransactionReceive;

            SelectedAccount.Total = balance;

            Transaction newTransaction = new(NewTransactionDate, NewTransactionPayee, NewTransactionCategory, new(NewTransactionSpend.Value), new(NewTransactionReceive.Value), new(balance.Value), NewTransactionMemo);
            SelectedAccountTransactions.Add(newTransaction);
            NewTransactionDate = DateTime.Today;
            NewTransactionPayee = "";
            NewTransactionCategory = "";
            NewTransactionSpend = new(0m);
            NewTransactionReceive = new(0m);
            NewTransactionMemo = "";
        }

        partial void OnSelectedAccountChanged(Account? value)
        {
            OnPropertyChanged(nameof(SelectedAccountTransactions));

            if (SelectedAccount != null) IsInputEnabled = true;
            else IsInputEnabled = false;
        }

        [ObservableProperty]
        private bool _IsInputEnabled = false;
    }
}
