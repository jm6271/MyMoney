using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Accounts;

public sealed class AccountValidationService : IAccountValidationService
{
    public const string InsufficientFunds = nameof(InsufficientFunds);

    public AccountValidationResult ValidateTransactionAmount(Currency amount, Account account)
    {
        if (amount.Value < 0 || Math.Abs(amount.Value) > account.Total.Value)
            return new(false, InsufficientFunds);

        return new(true);
    }
}
