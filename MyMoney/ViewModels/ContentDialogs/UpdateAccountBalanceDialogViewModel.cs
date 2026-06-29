using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Models;
using MyMoney.Core.Services;
using MyMoney.ValidationAttributes;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class UpdateAccountBalanceDialogViewModel : ObservableValidator
    {
        [ObservableProperty]
        private Currency _balance = new(0m);

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Balance is required.")]
        [CurrencyExpression(
            AllowNegative = false,
            RequiredErrorMessage = "Balance is required.",
            NegativeErrorMessage = "Balance cannot be negative.")]
        private string _balanceStr = "";

        public void Validate()
        {
            ValidateAllProperties();
        }

        private void FormatBalance()
        {
            ValidateProperty(BalanceStr, nameof(BalanceStr));

            if (GetErrors(nameof(BalanceStr)).Cast<object>().Any())
            {
                return;
            }

            BalanceStr = Balance.ToString();
        }

        partial void OnBalanceChanged(Currency value)
        {
            BalanceStr = value.ToString();
        }

        partial void OnBalanceStrChanged(string? oldValue, string newValue)
        {
            _balance = CurrencyExpressionParser.TryEvaluate(newValue, out decimal amount, out _)
                ? new Currency(amount)
                : new Currency(0m);
            FormatBalance();
        }
    }
}
