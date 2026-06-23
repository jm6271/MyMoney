using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Models;
using MyMoney.Core.Services;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class BudgetCategoryDialogViewModel : ObservableValidator
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Budget category is required.")]
        [CustomValidation(typeof(BudgetCategoryDialogViewModel), nameof(ValidateBudgetCategory))]
        private string _budgetCategory = "";

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Budget amount is required.")]
        [CustomValidation(typeof(BudgetCategoryDialogViewModel), nameof(ValidateBudgetedAmount))]
        private string _budgetAmountStr = "";

        private Currency _budgetedAmount = new Currency(0);

        public Currency BudgetAmount
        {
            get
            {
                return _budgetedAmount;
            }
            set
            {
                if (_budgetedAmount != value)
                {
                    _budgetedAmount = value;
                    BudgetAmountStr = value.ToString();
                }
            }
        }

        public void Validate()
        {
            ValidateAllProperties();
        }

        private void FormatBudgetAmount()
        {
            ValidateProperty(BudgetAmountStr, nameof(BudgetAmountStr));

            if (GetErrors(nameof(BudgetAmountStr)).Cast<object>().Any())
            {
                return;
            }

            BudgetAmountStr = BudgetAmount.ToString();
        }

        partial void OnBudgetAmountStrChanged(string? oldValue, string newValue)
        {
            _budgetedAmount = CurrencyExpressionParser.TryEvaluate(newValue, out decimal amount, out _)
                ? new Currency(amount)
                : new Currency(0);
            FormatBudgetAmount();
        }

        public static ValidationResult? ValidateBudgetCategory(object? value, ValidationContext context)
        {
            var category = value as string;

            if (string.IsNullOrWhiteSpace(category))
            {
                return new ValidationResult("Budget category is required.");
            }

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateBudgetedAmount(object? value, ValidationContext context)
        {
            var expression = value as string;

            if (string.IsNullOrWhiteSpace(expression))
            {
                return new ValidationResult("Amount is required.");
            }

            if (!CurrencyExpressionParser.TryEvaluate(
                    expression,
                    out decimal amount,
                    out string? error))
            {
                return new ValidationResult(
                    error ?? "Enter a valid amount or math expression.");
            }

            if (amount < 0)
            {
                return new ValidationResult(
                    "The amount cannot be negative.");
            }

            return ValidationResult.Success;
        }
    }
}
