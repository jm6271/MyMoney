namespace MyMoney.Core.Services.Budgets;

public sealed record SavingsCategorySyncResult(bool Changed, string? Reason = null);
