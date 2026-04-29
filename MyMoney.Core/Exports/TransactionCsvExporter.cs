using System.Globalization;
using CsvHelper;
using MyMoney.Core.Database;
using MyMoney.Core.Models;

namespace MyMoney.Core.Exports;

public class TransactionCsvExporter : ITransactionCsvExporter
{
    private readonly IDatabaseManager _databaseManager;

    public TransactionCsvExporter(IDatabaseManager databaseManager)
    {
        _databaseManager = databaseManager ?? throw new ArgumentNullException(nameof(databaseManager));
    }

    public async Task<TransactionCsvExportResult> ExportAsync(TransactionCsvExportOptions options)
    {
        if (options.SelectedFields.Count == 0)
        {
            return TransactionCsvExportResult.Failed("At least one field must be selected.");
        }

        if (options.UseDateRange && (!options.StartDate.HasValue || !options.EndDate.HasValue))
        {
            return TransactionCsvExportResult.Failed("A start and end date are required for date-range exports.");
        }

        try
        {
            var transactions = await GetTransactionsAsync(options);
            var accounts = _databaseManager.GetCollection<Account>("Accounts")
                .ToDictionary(a => a.Id, a => a.AccountName);

            await using var writer = new StreamWriter(options.OutputFilePath);
            await using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);

            foreach (var field in options.SelectedFields)
            {
                csv.WriteField(GetHeader(field));
            }
            await csv.NextRecordAsync();

            foreach (var transaction in transactions)
            {
                foreach (var field in options.SelectedFields)
                {
                    csv.WriteField(GetValue(field, transaction, accounts));
                }
                await csv.NextRecordAsync();
            }

            return TransactionCsvExportResult.Succeeded(transactions.Count);
        }
        catch (Exception ex)
        {
            return TransactionCsvExportResult.Failed(ex.Message);
        }
    }

    private async Task<List<Transaction>> GetTransactionsAsync(TransactionCsvExportOptions options)
    {
        List<Transaction> transactions = [];

        await _databaseManager.QueryAsync<Transaction>("Transactions", query =>
        {
            var filtered = query;

            if (!options.ExportAllAccounts && options.AccountId.HasValue)
            {
                var accountId = options.AccountId.Value;
                filtered = filtered.Where(t => t.AccountId == accountId);
            }

            if (options.UseDateRange && options.StartDate.HasValue && options.EndDate.HasValue)
            {
                var start = options.StartDate.Value.Date;
                var end = options.EndDate.Value.Date;
                filtered = filtered.Where(t => t.Date >= start && t.Date <= end);
            }

            transactions = filtered.OrderBy(t => t.Date).ToList();

            return Task.CompletedTask;
        });

        return transactions;
    }

    private static string GetHeader(TransactionExportField field) =>
        field switch
        {
            TransactionExportField.AccountName => "Account Name",
            TransactionExportField.Reconciled => "Reconciled",
            TransactionExportField.Date => "Date",
            TransactionExportField.Payee => "Payee",
            TransactionExportField.Category => "Category",
            TransactionExportField.Amount => "Amount",
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };

    private static object GetValue(
        TransactionExportField field,
        Transaction transaction,
        IReadOnlyDictionary<int, string> accountNames
    ) =>
        field switch
        {
            TransactionExportField.AccountName => accountNames.TryGetValue(transaction.AccountId, out var name) ? name : "",
            TransactionExportField.Reconciled => transaction.Reconciled,
            TransactionExportField.Date => transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TransactionExportField.Payee => transaction.Payee,
            TransactionExportField.Category => transaction.Category?.Name ?? "",
            TransactionExportField.Amount => transaction.Amount.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };
}
