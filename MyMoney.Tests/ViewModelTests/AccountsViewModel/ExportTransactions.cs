using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Exports;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

[TestClass]
public class ExportTransactionsTests
{
    private Mock<IContentDialogService> _contentDialogServiceMock = null!;
    private Mock<IMessageBoxService> _messageBoxServiceMock = null!;
    private Mock<IContentDialogFactory> _contentDialogFactoryMock = null!;
    private Mock<IFileDialogService> _fileDialogServiceMock = null!;
    private Mock<ITransactionCsvExporter> _transactionCsvExporterMock = null!;
    private DatabaseManager _databaseManager = null!;
    private AccountsViewModel _viewModel = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _contentDialogServiceMock = new Mock<IContentDialogService>();
        _messageBoxServiceMock = new Mock<IMessageBoxService>();
        _contentDialogFactoryMock = new Mock<IContentDialogFactory>();
        _fileDialogServiceMock = new Mock<IFileDialogService>();
        _transactionCsvExporterMock = new Mock<ITransactionCsvExporter>();

        _databaseManager = new DatabaseManager(new MemoryStream());
        _databaseManager.WriteCollection("Accounts", [
            new Account { AccountName = "Checking", Total = new Currency(1000m), Id = 1 },
        ]);

        _viewModel = new AccountsViewModel(
            _contentDialogServiceMock.Object,
            _databaseManager,
            _messageBoxServiceMock.Object,
            _contentDialogFactoryMock.Object,
            _fileDialogServiceMock.Object,
            _transactionCsvExporterMock.Object
        );

        await _viewModel.OnNavigatedToAsync();
        _viewModel.SelectedAccount = _viewModel.Accounts[0];
        _viewModel.SelectedAccountIndex = 0;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _databaseManager.Dispose();
    }

    [TestMethod]
    public async Task ExportTransactions_DialogCancelled_DoesNotInvokeExporter()
    {
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Secondary);

        _contentDialogFactoryMock.Setup(x => x.Create<ExportTransactionsDialog>()).Returns(fake.Object);

        await _viewModel.ExportTransactionsCommand.ExecuteAsync(null);

        _transactionCsvExporterMock.Verify(x => x.ExportAsync(It.IsAny<TransactionCsvExportOptions>()), Times.Never);
    }

    [TestMethod]
    public async Task ExportTransactions_SaveDialogCancelled_DoesNotInvokeExporter()
    {
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<ExportTransactionsDialog>()).Returns(fake.Object);
        _fileDialogServiceMock
            .Setup(x => x.ShowSaveCsvFileDialog(It.IsAny<string>()))
            .Returns((string?)null);

        await _viewModel.ExportTransactionsCommand.ExecuteAsync(null);

        _transactionCsvExporterMock.Verify(x => x.ExportAsync(It.IsAny<TransactionCsvExportOptions>()), Times.Never);
    }

    [TestMethod]
    public async Task ExportTransactions_Success_UsesDialogOptionsAndCallsExporter()
    {
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                var vm = fake.Object.DataContext as ExportTransactionsDialogViewModel;
                vm!.ExportCurrentAccount = true;
                vm.ExportAllAccounts = false;
                vm.UseDateRange = true;
                vm.StartDate = new DateTime(2026, 4, 1);
                vm.EndDate = new DateTime(2026, 4, 30);
                vm.IncludeAccountName = false;
            })
            .ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<ExportTransactionsDialog>()).Returns(fake.Object);
        _fileDialogServiceMock.Setup(x => x.ShowSaveCsvFileDialog(It.IsAny<string>())).Returns("C:\\temp\\out.csv");
        _transactionCsvExporterMock
            .Setup(x => x.ExportAsync(It.IsAny<TransactionCsvExportOptions>()))
            .ReturnsAsync(TransactionCsvExportResult.Succeeded(4));

        await _viewModel.ExportTransactionsCommand.ExecuteAsync(null);

        _transactionCsvExporterMock.Verify(
            x => x.ExportAsync(
                It.Is<TransactionCsvExportOptions>(o =>
                    !o.ExportAllAccounts
                    && o.AccountId == 1
                    && o.UseDateRange
                    && o.StartDate == new DateTime(2026, 4, 1)
                    && o.EndDate == new DateTime(2026, 4, 30)
                    && o.OutputFilePath == "C:\\temp\\out.csv"
                    && !o.SelectedFields.Contains(TransactionExportField.AccountName)
                )
            ),
            Times.Once
        );
    }
}
