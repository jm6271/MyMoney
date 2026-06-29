using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Models;
using MyMoney.Core.Services;
using MyMoney.ValidationAttributes;

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
        [CurrencyExpression(AllowNegative = false)]
        private string _budgetAmountStr = new Currency(0m).ToString();

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

    }
}
