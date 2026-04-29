namespace MyMoney.Core.Exports;

public class TransactionCsvExportResult
{
    public bool Success { get; init; }

    public int RowCount { get; init; }

    public string? ErrorMessage { get; init; }

    public static TransactionCsvExportResult Succeeded(int rowCount) => new() { Success = true, RowCount = rowCount };

    public static TransactionCsvExportResult Failed(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
