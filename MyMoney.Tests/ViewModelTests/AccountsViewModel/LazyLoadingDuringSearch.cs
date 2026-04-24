using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.AccountsViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[TestClass]
public class LazyLoadingDuringSearch
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.AccountsViewModel _viewModel;

    private Account _testAccount;
    private List<Transaction> _testTransactions;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseService = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        // Setup test account
        _testAccount = new Account { Id = 1, AccountName = "Test Account", Total = new Currency(1000m) };

        // Setup test transactions
        _testTransactions = new List<Transaction>();
        for (int i = 0; i < 50; i++)
        {
            _testTransactions.Add(new Transaction(
                DateTime.Now.AddDays(-i),
                "Payee " + i,
                new Category { Name = "Category " + (i % 5), Group = "Group " + (i % 3) },
                new Currency((decimal)(i * 10)), // Fixed: cast to decimal
                "Memo " + i
            ) { AccountId = 1, Id = i + 1 });
        }

        // Mock database operations
        _mockDatabaseService.Setup(dm => dm.GetCollection<Account>("Accounts"))
            .Returns(new List<Account> { _testAccount });

        _mockDatabaseService.Setup(dm => dm.SearchTransactionsAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((int accountId, string query) =>
                _testTransactions.Where(t =>
                    t.Payee.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    t.Category.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(t.Memo) && t.Memo.Contains(query, StringComparison.OrdinalIgnoreCase))
                ).ToList()
            );

        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseService.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );
    }

    [TestMethod]
    public async Task LoadTransactions_ShouldNotLoad_WhenSearchIsActive()
    {
        // Arrange
        await _viewModel.SelectedAccountChanged();

        // Act - Perform a search
        _viewModel.SearchQuery = "Payee 1";
        await Task.Delay(350); // Wait for debounce

        // Verify search is active
        var transactionsBefore = _viewModel.SelectedAccountTransactions.Count;

        // Act - Try to load more transactions (simulate scroll)
        await _viewModel.LoadTransactions();

        // Assert - Transactions count should not change
        Assert.AreEqual(transactionsBefore, _viewModel.SelectedAccountTransactions.Count,
            "Transaction count should not change when loading during active search");
    }
}