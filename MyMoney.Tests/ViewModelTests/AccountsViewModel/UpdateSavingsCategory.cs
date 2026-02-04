using System.Threading.Tasks;
using LiteDB;
using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Tests;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[TestClass]
public class UpdateSavingsCategoryTests
{
    private Mock<IContentDialogService> _contentDialogServiceMock;
    private Mock<IMessageBoxService> _messageBoxServiceMock;
    private Mock<IContentDialogFactory> _contentDialogFactoryMock;
    private ViewModels.Pages.AccountsViewModel _viewModel;

    // Common test objects
    private DatabaseManager _databaseManager;

    [TestInitialize]
    public async Task Setup()
    {
        _contentDialogServiceMock = new Mock<IContentDialogService>();
        _messageBoxServiceMock = new Mock<IMessageBoxService>();
        _contentDialogFactoryMock = new Mock<IContentDialogFactory>();

        _databaseManager = new(new MemoryStream());
        _databaseManager.WriteCollection("Accounts", [
            new Account() { AccountName = "Test Account", Total = new(1000), Id = 1 }
        ]);

        _databaseManager.WriteCollection("Budgets", [
            new Budget()
            {
                BudgetDate = DateTime.Today,
                BudgetTitle = DateTime.Today.ToString("MMMM, yyyy"),
                BudgetSavingsCategories = [new() { CategoryName = "Test Category", CurrentBalance = new(500) }],
            }
        ]);

        _viewModel = new ViewModels.Pages.AccountsViewModel(
            _contentDialogServiceMock.Object,
            _databaseManager,
            _messageBoxServiceMock.Object,
            _contentDialogFactoryMock.Object
        );

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        // Common view model selection setup
        await _viewModel.OnNavigatedToAsync();
        _viewModel.SelectedAccountIndex = 0;
        _viewModel.SelectedAccount = _viewModel.Accounts[0];
    }

    [TestMethod]
    public async Task Test_UpdateSavingsCategory_NewTransaction_UpdatesSavingsCategory()
    {
        // Arrange
        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewTransactionDialogViewModel;
                    vm?.NewTransactionAmount = new Currency(100);
                    vm?.NewTransactionIsExpense = true;
                    vm?.NewTransactionIsIncome = false;
                    vm?.NewTransactionDate = DateTime.Today;
                    vm?.NewTransactionCategory = new() { Name = "Test Category", Group = "Savings" };
                    vm?.NewTransactionPayee = "Test Payee";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        // Act
        await _viewModel.CreateNewTransactionCommand.ExecuteAsync(null);

        var finalBudgetCollection = new BudgetCollection(_databaseManager);

        // Assert
        Assert.HasCount(1, finalBudgetCollection.Budgets);
        await _viewModel.SelectedAccountChanged();
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
        Assert.HasCount(1, finalBudgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions);
        Assert.AreEqual(
            _viewModel.SelectedAccountTransactions[0].TransactionHash,
            finalBudgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions[0].TransactionHash
        );
        Assert.AreEqual(new(400), finalBudgetCollection.Budgets[0].BudgetSavingsCategories[0].CurrentBalance);
    }

    [TestMethod]
    public async Task UpdateSavingsCategory_EditTransaction_SameCategoryName_UpdatesSavingsCategory()
    {
        // Arrange
        // Add a transaction to edit
        Transaction originalTransaction = new(
            DateTime.Today,
            "Original Payee",
            new() { Group = "Savings", Name = "Test Category" },
            new(-200),
            "Original Memo"
        );
        _viewModel.SelectedAccountTransactions.Add(originalTransaction);

        var budgetCollection = new BudgetCollection(_databaseManager);
        budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions.Add(originalTransaction);
        budgetCollection.SaveBudgetCollection();

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewTransactionDialogViewModel;
                    vm?.NewTransactionAmount = new Currency(100);
                    vm?.NewTransactionIsExpense = true;
                    vm?.NewTransactionIsIncome = false;
                    vm?.NewTransactionDate = DateTime.Today;
                    vm?.NewTransactionCategory = new() { Name = "Test Category", Group = "Savings" };
                    vm?.NewTransactionPayee = "New Payee";
                    vm?.NewTransactionMemo = "New Memo";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        _viewModel.SelectedTransactionIndex = 0;
        _viewModel.SelectedTransaction = _viewModel.SelectedAccountTransactions[0];

        // Act
        await _viewModel.EditTransactionCommand.ExecuteAsync(null);

        budgetCollection = new(_databaseManager);

        // Assert
        Assert.HasCount(1, _viewModel.Accounts);
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
        Assert.HasCount(1, budgetCollection.Budgets);
        Assert.HasCount(1, budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions);
        Assert.AreEqual(
            _viewModel.SelectedAccountTransactions[0].TransactionHash,
            budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions[0].TransactionHash
        );
        Assert.AreEqual(-100m, _viewModel.SelectedAccountTransactions[0].Amount.Value);
        Assert.AreEqual(-100m, budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions[0].Amount.Value);
        Assert.AreEqual(600m, budgetCollection.Budgets[0].BudgetSavingsCategories[0].CurrentBalance.Value);
    }

    [TestMethod]
    public async Task UpdateSavingsCategory_EditTransaction_DifferentCategoryName_UpdatesSavingsCategory()
    {
        // Arrange

        // Modify the test budget to have two categories
        var budgetCollection = new BudgetCollection(_databaseManager);
        budgetCollection.Budgets[0].BudgetSavingsCategories.Add(new() { CategoryName = "Other Category", CurrentBalance = new(1000) });

        // Add a transaction to edit
        Transaction originalTransaction = new(
            DateTime.Today,
            "Original Payee",
            new() { Group = "Savings", Name = "Test Category" },
            new(-100),
            "Original Memo"
        );
        _viewModel.SelectedAccountTransactions.Add(originalTransaction);
        budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions.Add(originalTransaction);
        budgetCollection.SaveBudgetCollection();

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewTransactionDialogViewModel;
                    vm?.NewTransactionAmount = new Currency(100);
                    vm?.NewTransactionIsExpense = true;
                    vm?.NewTransactionIsIncome = false;
                    vm?.NewTransactionDate = DateTime.Today;
                    vm?.NewTransactionCategory = new() { Name = "Other Category", Group = "Savings" };
                    vm?.NewTransactionPayee = "Test Payee";
                    vm?.NewTransactionMemo = "New Memo";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _contentDialogFactoryMock.Setup(x => x.Create<NewTransactionDialog>()).Returns(fake.Object);

        _viewModel.SelectedTransactionIndex = 0;
        _viewModel.SelectedTransaction = _viewModel.SelectedAccountTransactions[0];

        // Act
        await _viewModel.EditTransactionCommand.ExecuteAsync(null);

        budgetCollection = new(_databaseManager);

        // Assert
        Assert.HasCount(1, _viewModel.Accounts);
        var account = _viewModel.Accounts[0];
        Assert.HasCount(1, _viewModel.SelectedAccountTransactions);
        Assert.HasCount(1, budgetCollection.Budgets);
        Assert.HasCount(0, budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions);
        Assert.HasCount(1, budgetCollection.Budgets[0].BudgetSavingsCategories[1].Transactions);
        Assert.AreEqual(
            _viewModel.SelectedAccountTransactions[0].TransactionHash,
            budgetCollection.Budgets[0].BudgetSavingsCategories[1].Transactions[0].TransactionHash
        );
        Assert.AreEqual(-100m, _viewModel.SelectedAccountTransactions[0].Amount.Value);
        Assert.AreEqual(-100m, budgetCollection.Budgets[0].BudgetSavingsCategories[1].Transactions[0].Amount.Value);
        Assert.AreEqual(600m, budgetCollection.Budgets[0].BudgetSavingsCategories[0].CurrentBalance.Value);
        Assert.AreEqual(900m, budgetCollection.Budgets[0].BudgetSavingsCategories[1].CurrentBalance.Value);
    }

    [TestMethod]
    public async Task UpdateSavingsCategory_DeleteTransaction_UpdatesSavingsCategory()
    {
        // Arrange

        // Add a transaction to delete
        Transaction transactionToDelete = new(
            DateTime.Today,
            "Payee",
            new() { Group = "Savings", Name = "Test Category" },
            new(-200),
            "Memo"
        );
        _viewModel.SelectedAccountTransactions.Add(transactionToDelete);
        BudgetCollection budgetCollection = new(_databaseManager);
        budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions.Add(transactionToDelete);
        budgetCollection.SaveBudgetCollection();

        _messageBoxServiceMock
            .Setup(x => x.ShowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(MessageBoxResult.Primary);

        _viewModel.SelectedTransactionIndex = 0;
        _viewModel.SelectedTransaction = _viewModel.SelectedAccountTransactions[0];

        // Act
        await _viewModel.DeleteTransactionCommand.ExecuteAsync(null);

        budgetCollection = new(_databaseManager);

        // Assert
        Assert.HasCount(0, _viewModel.SelectedAccountTransactions);
        Assert.HasCount(1, budgetCollection.Budgets);
        Assert.HasCount(0, budgetCollection.Budgets[0].BudgetSavingsCategories[0].Transactions);
        Assert.AreEqual(700m, budgetCollection.Budgets[0].BudgetSavingsCategories[0].CurrentBalance.Value);
    }
}
