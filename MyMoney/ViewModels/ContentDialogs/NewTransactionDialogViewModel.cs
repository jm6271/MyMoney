using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Views.Controls;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewTransactionDialogViewModel : ObservableObject
    {
        private readonly object _databaseLockObject = new();
        private readonly IDatabaseManager _databaseManager;

        [ObservableProperty]
        private DateTime _newTransactionDate = DateTime.Today;

        [ObservableProperty]
        private string _newTransactionPayee = "";

        [ObservableProperty]
        private Category _newTransactionCategory = new();

        [ObservableProperty]
        private int _newTransactionCategorySelectedIndex = -1;

        [ObservableProperty]
        private string _newTransactionMemo = "";

        [ObservableProperty]
        private Currency _newTransactionAmount = new(0m);

        [ObservableProperty]
        private bool _newTransactionIsExpense = true;

        [ObservableProperty]
        private bool _newTransactionIsIncome;

        [ObservableProperty]
        private List<string> _autoSuggestPayees = [];

        [ObservableProperty]
        private int _selectedAccountIndex = 0;

        [ObservableProperty]
        private ObservableCollection<Account> _accounts = [];

        [ObservableProperty]
        private Account? _selectedAccount;

        [ObservableProperty]
        private Visibility _accountsVisibility = Visibility.Visible;

        [ObservableProperty]
        private ObservableCollection<BudgetCategory> _categoryNames;

        public ICollectionView CategoriesView { get; }

        public NewTransactionDialogViewModel(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager ?? throw new ArgumentNullException(nameof(databaseManager));

            CategoryNames = BudgetCategoryNames;

            CategoriesView = CollectionViewSource.GetDefaultView(CategoryNames);
            CategoriesView.GroupDescriptions.Add(
                new PropertyGroupDescription(nameof(BudgetCategory.Group)));
        }

        public void SetSelectedCategoryByName(string categoryName)
        {
            var categoryItem = CategoryNames.FirstOrDefault(c => c.Item.ToString() == categoryName);

            if (categoryItem is not null)
            {
                NewTransactionCategorySelectedIndex = CategoryNames.IndexOf(categoryItem);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(NewTransactionDate))
            {
                CategoryNames.Clear();

                foreach (var item in BudgetCategoryNames)
                {
                    CategoryNames.Add(item);
                }
            }
        }

        public ObservableCollection<BudgetCategory> BudgetCategoryNames
        {
            get
            {
                var categories = new ObservableCollection<BudgetCategory>();

                BudgetCollection budgetCollection;
                lock (_databaseLockObject)
                {
                    budgetCollection = new(_databaseManager);
                    var budget = budgetCollection.Budgets.FirstOrDefault(b =>
                        b.BudgetDate.Month == NewTransactionDate.Month && b.BudgetDate.Year == NewTransactionDate.Year
                    );

                    if (budget == null)
                        return categories;

                    AddCategoriesToCollection(categories, "Income", budget.BudgetIncomeItems.Select(x => x.Category));
                    AddCategoriesToCollection(
                        categories,
                        "Savings",
                        budget.BudgetSavingsCategories.Select(x => x.CategoryName)
                    );

                    foreach (var expenseGroup in budget.BudgetExpenseItems)
                    {
                        AddCategoriesToCollection(
                            categories,
                            expenseGroup.CategoryName,
                            expenseGroup.SubItems.Select(x => x.Category)
                        );
                    }
                }

                return categories;
            }
        }

        private void AddCategoriesToCollection(
            ObservableCollection<BudgetCategory> collection,
            string group,
            IEnumerable<string> items
        )
        {
            foreach (var item in items)
            {
                collection.Add(new BudgetCategory { Group = group, Item = item });
            }
        }
    }
}
