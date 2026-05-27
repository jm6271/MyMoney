namespace MyMoney.Core.Services.Accounts;

public sealed record TransactionPageRequest(int AccountId, DateTime? Before, int BeforeId, int PageSize);
