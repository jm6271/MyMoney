using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Models;
using MyMoney.Core.Services;
using MyMoney.ValidationAttributes;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class SavingsCategoryDialogViewModel : ObservableValidator
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Savings category is required.")]
        [CustomValidation(typeof(SavingsCategoryDialogViewModel), nameof(ValidateSavingsCategory))]
        private string _category = string.Empty;

        [ObservableProperty]
        private Currency _planned = new(0m);

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Planned amount is required.")]
        [CurrencyExpression(
            AllowNegative = false,
            RequiredErrorMessage = "Planned amount is required.",
            NegativeErrorMessage = "Planned amount cannot be negative.")]
        private string _plannedStr = new Currency(0m).ToString();

        [ObservableProperty]
        private Currency _currentBalance = new(0m);

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Current balance is required.")]
        [CurrencyExpression(
            AllowNegative = false,
            RequiredErrorMessage = "Current balance is required.",
            NegativeErrorMessage = "Current balance cannot be negative.")]
        private string _currentBalanceStr = new Currency(0m).ToString();

        [ObservableProperty]
        private ObservableCollection<Transaction> _recentTransactions = [];

        [ObservableProperty]
        private Visibility _recentTransactionsVisibility = Visibility.Visible;

        [ObservableProperty]
        private GridLength _recentTransactionsColumnWidth = new(1, GridUnitType.Star);

        [ObservableProperty]
        private GridLength _editColumnWidth = new(200, GridUnitType.Pixel);

        public void Validate()
        {
            ValidateAllProperties();
        }

        public void SortTransactions()
        {
            // Sort the transactions
            var sortedTransactions = RecentTransactions.OrderByDescending(x => x.Date).ToList();
            RecentTransactions.Clear();
            foreach (var transaction in sortedTransactions)
                RecentTransactions.Add(transaction);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(RecentTransactionsVisibility))
            {
                if (RecentTransactionsVisibility == Visibility.Collapsed)
                {
                    RecentTransactionsColumnWidth = new(0, GridUnitType.Pixel);
                    EditColumnWidth = new(1, GridUnitType.Star);
                }
                else
                {
                    RecentTransactionsColumnWidth = new(1, GridUnitType.Star);
                    EditColumnWidth = new(200, GridUnitType.Pixel);
                }
            }
        }

        private void FormatPlanned()
        {
            ValidateProperty(PlannedStr, nameof(PlannedStr));

            if (GetErrors(nameof(PlannedStr)).Cast<object>().Any())
            {
                return;
            }

            PlannedStr = Planned.ToString();
        }

        private void FormatCurrentBalance()
        {
            ValidateProperty(CurrentBalanceStr, nameof(CurrentBalanceStr));

            if (GetErrors(nameof(CurrentBalanceStr)).Cast<object>().Any())
            {
                return;
            }

            CurrentBalanceStr = CurrentBalance.ToString();
        }

        partial void OnPlannedChanged(Currency value)
        {
            PlannedStr = value.ToString();
        }

        partial void OnPlannedStrChanged(string? oldValue, string newValue)
        {
            _planned = CurrencyExpressionParser.TryEvaluate(newValue, out decimal amount, out _)
                ? new Currency(amount)
                : new Currency(0m);
            FormatPlanned();
        }

        partial void OnCurrentBalanceChanged(Currency value)
        {
            CurrentBalanceStr = value.ToString();
        }

        partial void OnCurrentBalanceStrChanged(string? oldValue, string newValue)
        {
            _currentBalance = CurrencyExpressionParser.TryEvaluate(newValue, out decimal amount, out _)
                ? new Currency(amount)
                : new Currency(0m);
            FormatCurrentBalance();
        }

        public static ValidationResult? ValidateSavingsCategory(object? value, ValidationContext context)
        {
            var category = value as string;

            if (string.IsNullOrWhiteSpace(category))
            {
                return new ValidationResult("Savings category is required.");
            }

            return ValidationResult.Success;
        }
    }
}
