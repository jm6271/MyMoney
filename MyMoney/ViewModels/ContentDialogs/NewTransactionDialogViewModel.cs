using MyMoney.Core.Models;
using MyMoney.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewTransactionDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _newTransactionDate = DateTime.Today;

        [ObservableProperty]
        private string _newTransactionPayee = "";

        [ObservableProperty]
        private Category _newTransactionCategory = new();

        [ObservableProperty]
        private string _newTransactionMemo = "";

        [ObservableProperty]
        private Currency _newTransactionAmount = new(0m);

        [ObservableProperty]
        private bool _newTransactionIsExpense = true;

        [ObservableProperty]
        private bool _newTransactionIsIncome;

        [ObservableProperty]
        private ObservableCollection<string> _autoSuggestPayees = [];

        [ObservableProperty]
        private int _selectedAccountIndex = 0;

        [ObservableProperty]
        private ObservableCollection<Account> _accounts = [];

        [ObservableProperty]
        private Account? _selectedAccount;

        [ObservableProperty]
        private ObservableCollection<GroupedComboBox.GroupedComboBoxItem> _categoryNames = [];

        [ObservableProperty]
        private Visibility _accountsVisibility = Visibility.Visible;
    }
}
