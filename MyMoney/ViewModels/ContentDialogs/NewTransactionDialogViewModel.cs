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
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewTransactionDialogViewModel : ObservableObject
    {
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
        private ObservableCollection<Category> _categoryNames = [];

        public ICollectionView? CategoriesView { get; set; }

        public NewTransactionDialogViewModel(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager ?? throw new ArgumentNullException(nameof(databaseManager));
        }

        public void SetCategoryNames(ObservableCollection<Category> categories)
        {
            CategoryNames = categories;

            CategoriesView = CollectionViewSource.GetDefaultView(CategoryNames);
            CategoriesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Category.Group)));
        }

        public void SetSelectedCategoryByName(string categoryName)
        {
            var categoryItem = CategoryNames.FirstOrDefault(c => c.Name == categoryName);

            if (categoryItem is not null)
            {
                NewTransactionCategorySelectedIndex = CategoryNames.IndexOf(categoryItem);
            }
        }

        partial void OnNewTransactionDateChanged(DateTime oldValue, DateTime newValue)
        {
            if (oldValue.Month == newValue.Month && oldValue.Year == newValue.Year)
                return;

            var selectedCategory = NewTransactionCategory.Name;

            CategoryNames.Clear();

            foreach (var item in GetBudgetCategoryNames())
            {
                CategoryNames.Add(item);
            }

            SetSelectedCategoryByName(selectedCategory);
        }

        public ObservableCollection<Category> GetBudgetCategoryNames()
        {
            var categories = new ObservableCollection<Category>();

            BudgetCollection budgetCollection;
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

            return categories;
        }

        private void AddCategoriesToCollection(
            ObservableCollection<Category> collection,
            string group,
            IEnumerable<string> items
        )
        {
            foreach (var item in items)
            {
                collection.Add(new Category { Group = group, Name = item });
            }
        }
    }
}
