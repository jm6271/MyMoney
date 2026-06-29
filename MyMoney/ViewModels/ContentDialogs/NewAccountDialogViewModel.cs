using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Models;
using MyMoney.Core.Services;
using MyMoney.ValidationAttributes;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewAccountDialogViewModel : ObservableValidator
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Account name is required.")]
        [CustomValidation(typeof(NewAccountDialogViewModel), nameof(ValidateAccountName))]
        private string _accountName = "";

        [ObservableProperty]
        private Currency _startingBalance = new(0m);

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Starting balance is required.")]
        [CurrencyExpression(
            AllowNegative = false,
            RequiredErrorMessage = "Starting balance is required.",
            NegativeErrorMessage = "Starting balance cannot be negative.")]
        private string _startingBalanceStr = new Currency(0m).ToString();

        public void Validate()
        {
            ValidateAllProperties();
        }

        private void FormatStartingBalance()
        {
            ValidateProperty(StartingBalanceStr, nameof(StartingBalanceStr));

            if (GetErrors(nameof(StartingBalanceStr)).Cast<object>().Any())
            {
                return;
            }

            StartingBalanceStr = StartingBalance.ToString();
        }

        partial void OnStartingBalanceChanged(Currency value)
        {
            StartingBalanceStr = value.ToString();
        }

        partial void OnStartingBalanceStrChanged(string? oldValue, string newValue)
        {
            _startingBalance = CurrencyExpressionParser.TryEvaluate(newValue, out decimal amount, out _)
                ? new Currency(amount)
                : new Currency(0m);
            FormatStartingBalance();
        }

        public static ValidationResult? ValidateAccountName(object? value, ValidationContext context)
        {
            var accountName = value as string;

            if (string.IsNullOrWhiteSpace(accountName))
            {
                return new ValidationResult("Account name is required.");
            }

            return ValidationResult.Success;
        }
    }
}
