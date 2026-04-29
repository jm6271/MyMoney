namespace MyMoney.Core.Exports;

public enum TransactionExportField
{
    AccountName,
    Reconciled,
    Date,
    Payee,
    Category,
    Amount,
}

public class TransactionCsvExportOptions
{
    public bool ExportAllAccounts { get; set; }

    public int? AccountId { get; set; }

    public bool UseDateRange { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public IReadOnlyList<TransactionExportField> SelectedFields { get; set; } = [];

    public required string OutputFilePath { get; init; }
}
