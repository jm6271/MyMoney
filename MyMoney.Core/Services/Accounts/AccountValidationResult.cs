namespace MyMoney.Core.Services.Accounts;

public sealed record AccountValidationResult(bool IsValid, string? ErrorCode = null);
