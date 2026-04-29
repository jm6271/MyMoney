namespace MyMoney.Core.Exports;

public interface ITransactionCsvExporter
{
    Task<TransactionCsvExportResult> ExportAsync(TransactionCsvExportOptions options);
}
