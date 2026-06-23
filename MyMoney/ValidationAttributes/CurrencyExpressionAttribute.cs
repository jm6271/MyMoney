using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Services;

namespace MyMoney.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class CurrencyExpressionAttribute : ValidationAttribute
    {
        public bool AllowNegative { get; set; } = true;

        public string RequiredErrorMessage { get; set; } = "Amount is required.";

        public string InvalidExpressionErrorMessage { get; set; } = "Enter a valid amount or math expression.";

        public string NegativeErrorMessage { get; set; } = "The amount cannot be negative.";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var expression = value as string;

            if (string.IsNullOrWhiteSpace(expression))
            {
                return new ValidationResult(RequiredErrorMessage);
            }

            if (!CurrencyExpressionParser.TryEvaluate(
                    expression,
                    out decimal amount,
                    out string? error))
            {
                return new ValidationResult(
                    error ?? InvalidExpressionErrorMessage);
            }

            if (!AllowNegative && amount < 0)
            {
                return new ValidationResult(
                    NegativeErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
