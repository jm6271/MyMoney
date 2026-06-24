using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Models;
using MyMoney.Core.Services;
using MyMoney.ValidationAttributes;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class TransferDialogViewModel(ObservableCollection<string> accountNames) : ObservableValidator
    {
        public ObservableCollection<string> Accounts { get; } = accountNames;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Transfer from account is required.")]
        [CustomValidation(typeof(TransferDialogViewModel), nameof(ValidateTransferFrom))]
        private string _transferFrom = "";

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Transfer to account is required.")]
        [CustomValidation(typeof(TransferDialogViewModel), nameof(ValidateTransferTo))]
        private string _transferTo = "";

        [ObservableProperty]
        private Currency _amount = new(0m);

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Transfer amount is required.")]
        [CurrencyExpression(
            AllowNegative = false,
            RequiredErrorMessage = "Transfer amount is required.",
            NegativeErrorMessage = "Transfer amount cannot be negative.")]
        private string _amountStr = new Currency(0m).ToString();

        public void Validate()
        {
            ValidateAllProperties();
        }

        private void FormatAmount()
        {
            ValidateProperty(AmountStr, nameof(AmountStr));

            if (GetErrors(nameof(AmountStr)).Cast<object>().Any())
            {
                return;
            }

            AmountStr = Amount.ToString();
        }

        partial void OnTransferFromChanged(string? oldValue, string newValue)
        {
            ValidateProperty(TransferTo, nameof(TransferTo));
        }

        partial void OnTransferToChanged(string? oldValue, string newValue)
        {
            ValidateProperty(TransferFrom, nameof(TransferFrom));
        }

        partial void OnAmountChanged(Currency value)
        {
            AmountStr = value.ToString();
        }

        partial void OnAmountStrChanged(string? oldValue, string newValue)
        {
            _amount = CurrencyExpressionParser.TryEvaluate(newValue, out decimal amount, out _)
                ? new Currency(amount)
                : new Currency(0m);
            FormatAmount();
        }

        public static ValidationResult? ValidateTransferFrom(object? value, ValidationContext context)
        {
            var transferFrom = value as string;

            if (string.IsNullOrWhiteSpace(transferFrom))
            {
                return new ValidationResult("Transfer from account is required.");
            }

            if (context.ObjectInstance is TransferDialogViewModel vm && transferFrom == vm.TransferTo)
            {
                return new ValidationResult("Transfer accounts must be different.");
            }

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateTransferTo(object? value, ValidationContext context)
        {
            var transferTo = value as string;

            if (string.IsNullOrWhiteSpace(transferTo))
            {
                return new ValidationResult("Transfer to account is required.");
            }

            if (context.ObjectInstance is TransferDialogViewModel vm && transferTo == vm.TransferFrom)
            {
                return new ValidationResult("Transfer accounts must be different.");
            }

            return ValidationResult.Success;
        }
    }
}
