using MyMoney.Core.Exports;

namespace MyMoney.ViewModels.ContentDialogs;

public partial class ExportTransactionsDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _exportCurrentAccount = true;

    [ObservableProperty]
    private bool _exportAllAccounts;

    [ObservableProperty]
    private bool _exportAllDates = true;

    [ObservableProperty]
    private bool _useDateRange;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private bool _includeAccountName = true;

    [ObservableProperty]
    private bool _includeReconciled = true;

    [ObservableProperty]
    private bool _includeDate = true;

    [ObservableProperty]
    private bool _includePayee = true;

    [ObservableProperty]
    private bool _includeCategory = true;

    [ObservableProperty]
    private bool _includeAmount = true;

    public bool HasSelectedFields =>
        IncludeAccountName
        || IncludeReconciled
        || IncludeDate
        || IncludePayee
        || IncludeCategory
        || IncludeAmount;

    public IReadOnlyList<TransactionExportField> GetSelectedFields()
    {
        var fields = new List<TransactionExportField>();

        if (IncludeAccountName)
            fields.Add(TransactionExportField.AccountName);
        if (IncludeReconciled)
            fields.Add(TransactionExportField.Reconciled);
        if (IncludeDate)
            fields.Add(TransactionExportField.Date);
        if (IncludePayee)
            fields.Add(TransactionExportField.Payee);
        if (IncludeCategory)
            fields.Add(TransactionExportField.Category);
        if (IncludeAmount)
            fields.Add(TransactionExportField.Amount);

        return fields;
    }

    partial void OnExportCurrentAccountChanged(bool value)
    {
        if (value)
            ExportAllAccounts = false;
    }

    partial void OnExportAllAccountsChanged(bool value)
    {
        if (value)
            ExportCurrentAccount = false;
    }

    partial void OnExportAllDatesChanged(bool value)
    {
        if (value)
            UseDateRange = false;
    }

    partial void OnUseDateRangeChanged(bool value)
    {
        if (value)
            ExportAllDates = false;
    }

    partial void OnIncludeAccountNameChanged(bool value) => OnPropertyChanged(nameof(HasSelectedFields));
    partial void OnIncludeReconciledChanged(bool value) => OnPropertyChanged(nameof(HasSelectedFields));
    partial void OnIncludeDateChanged(bool value) => OnPropertyChanged(nameof(HasSelectedFields));
    partial void OnIncludePayeeChanged(bool value) => OnPropertyChanged(nameof(HasSelectedFields));
    partial void OnIncludeCategoryChanged(bool value) => OnPropertyChanged(nameof(HasSelectedFields));
    partial void OnIncludeAmountChanged(bool value) => OnPropertyChanged(nameof(HasSelectedFields));
}
