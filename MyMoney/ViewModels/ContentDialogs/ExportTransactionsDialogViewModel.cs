using System.ComponentModel.DataAnnotations;
using MyMoney.Core.Exports;

namespace MyMoney.ViewModels.ContentDialogs;

public partial class ExportTransactionsDialogViewModel : ObservableValidator
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
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateDateRange))]
    private DateTime _startDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateDateRange))]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateSelectedFields))]
    private bool _includeAccountName = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateSelectedFields))]
    private bool _includeReconciled = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateSelectedFields))]
    private bool _includeDate = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateSelectedFields))]
    private bool _includePayee = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateSelectedFields))]
    private bool _includeCategory = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ExportTransactionsDialogViewModel), nameof(ValidateSelectedFields))]
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

    public void Validate()
    {
        ValidateAllProperties();
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

        ValidateDateRangeProperties();
    }

    partial void OnUseDateRangeChanged(bool value)
    {
        if (value)
            ExportAllDates = false;

        ValidateDateRangeProperties();
    }

    partial void OnStartDateChanged(DateTime value) => ValidateDateRangeProperties();

    partial void OnEndDateChanged(DateTime value) => ValidateDateRangeProperties();

    partial void OnIncludeAccountNameChanged(bool value) => ValidateFieldProperties();
    partial void OnIncludeReconciledChanged(bool value) => ValidateFieldProperties();
    partial void OnIncludeDateChanged(bool value) => ValidateFieldProperties();
    partial void OnIncludePayeeChanged(bool value) => ValidateFieldProperties();
    partial void OnIncludeCategoryChanged(bool value) => ValidateFieldProperties();
    partial void OnIncludeAmountChanged(bool value) => ValidateFieldProperties();

    private void ValidateDateRangeProperties()
    {
        ValidateProperty(StartDate, nameof(StartDate));
        ValidateProperty(EndDate, nameof(EndDate));
    }

    private void ValidateFieldProperties()
    {
        OnPropertyChanged(nameof(HasSelectedFields));
        ValidateProperty(IncludeAccountName, nameof(IncludeAccountName));
        ValidateProperty(IncludeReconciled, nameof(IncludeReconciled));
        ValidateProperty(IncludeDate, nameof(IncludeDate));
        ValidateProperty(IncludePayee, nameof(IncludePayee));
        ValidateProperty(IncludeCategory, nameof(IncludeCategory));
        ValidateProperty(IncludeAmount, nameof(IncludeAmount));
    }

    public static ValidationResult? ValidateDateRange(object? value, ValidationContext context)
    {
        if (context.ObjectInstance is not ExportTransactionsDialogViewModel viewModel)
        {
            return ValidationResult.Success;
        }

        return viewModel.UseDateRange && viewModel.StartDate.Date > viewModel.EndDate.Date
            ? new ValidationResult("Start date must be on or before end date.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidateSelectedFields(object? value, ValidationContext context)
    {
        if (context.ObjectInstance is not ExportTransactionsDialogViewModel viewModel)
        {
            return ValidationResult.Success;
        }

        return viewModel.HasSelectedFields
            ? ValidationResult.Success
            : new ValidationResult("Select at least one field.");
    }
}
