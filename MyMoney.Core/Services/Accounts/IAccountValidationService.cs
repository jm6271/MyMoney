using MyMoney.Core.Models;

namespace MyMoney.Core.Services.Accounts;

public interface IAccountValidationService
{
    AccountValidationResult ValidateTransactionAmount(Currency amount, Account account);
}
