using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel
{
    [TestClass]
    public class CreateNewBudgetTests
    {
        private Mock<IContentDialogService> _mockContentDialogService = null!;
        private Mock<IDatabaseReader> _mockDatabaseReader = null!;
        private Mock<IMessageBoxService> _mockMessageBoxService = null!;
        private Mock<INewBudgetDialogService> _mockNewBudgetDialogService = null!;
        private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService = null!;
        private Mock<INewExpenseGroupDialogService> _mockNewExpenseDialogService = null!;
        private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService = null!;
        private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockDatabaseReader = new Mock<IDatabaseReader>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
            _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
            _mockNewExpenseDialogService = new Mock<INewExpenseGroupDialogService>();
            _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget>());

            _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockMessageBoxService.Object,
                _mockNewBudgetDialogService.Object,
                _mockBudgetCategoryDialogService.Object,
                _mockNewExpenseDialogService.Object,
                _mockSavingsCategoryDialogService.Object
            );
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenBudgetDoesntExist_CreatesNewBudget()
        {
            // Arrange
            var budgetDate = DateTime.Now.AddMonths(1);
            var budgetTitle = budgetDate.ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var dialogViewModel = new NewBudgetDialogViewModel
            {
                SelectedDate = budgetTitle,
                UseLastMonthsBudget = false
            };

            _mockNewBudgetDialogService.Setup(x => x.GetViewModel())
                .Returns(dialogViewModel);
            _mockNewBudgetDialogService.Setup(x => x.ShowDialogAsync(_mockContentDialogService.Object))
                .ReturnsAsync(ContentDialogResult.Primary);

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(1, _viewModel.Budgets.Count);
            Assert.AreEqual(budgetTitle, _viewModel.Budgets[0].BudgetTitle);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetIncomeItems.Count);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetExpenseItems.Count);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetSavingsCategories.Count);
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenBudgetExistsAlready_ShowsWarningAndDoesNotCreate()
        {
            // Arrange
            var existingBudget = new Budget
            {
                BudgetTitle = DateTime.Now.AddMonths(1).ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture),
                BudgetDate = DateTime.Now.AddMonths(1)
            };

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget> { existingBudget });

            var dialogViewModel = new NewBudgetDialogViewModel
            {
                SelectedDate = existingBudget.BudgetTitle,
                UseLastMonthsBudget = false
            };

            _mockNewBudgetDialogService.Setup(x => x.GetViewModel())
                .Returns(dialogViewModel);
            _mockNewBudgetDialogService.Setup(x => x.ShowDialogAsync(_mockContentDialogService.Object))
                .ReturnsAsync(ContentDialogResult.Primary);

            _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockMessageBoxService.Object,
                _mockNewBudgetDialogService.Object,
                _mockBudgetCategoryDialogService.Object,
                _mockNewExpenseDialogService.Object,
                _mockSavingsCategoryDialogService.Object
            );

            _viewModel.OnPageNavigatedTo();
            _viewModel.CurrentBudget = _viewModel.Budgets[0];

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(1, _viewModel.Budgets.Count); // Only the existing budget
            _mockMessageBoxService.Verify(x => x.ShowInfoAsync(
                "Budget Already Exists",
                "Cannot create a budget for the selected month because a budget for this month already exists",
                "OK"
            ), Times.Once);
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenDialogResultNotPrimary_DoesNotCreateBudget()
        {
            // Arrange
            _mockNewBudgetDialogService.Setup(x => x.ShowDialogAsync(_mockContentDialogService.Object))
                .ReturnsAsync(ContentDialogResult.Secondary);

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(0, _viewModel.Budgets.Count);
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenCopyingFromCurrentBudget_CopiesAllItems()
        {
            // Arrange
            var currentBudgetDate = DateTime.Now;
            var nextBudgetDate = currentBudgetDate.AddMonths(1);
            var currentBudgetTitle = currentBudgetDate.ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var nextBudgetTitle = nextBudgetDate.ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var currentBudget = new Budget
            {
                BudgetTitle = currentBudgetTitle,
                BudgetDate = currentBudgetDate,
                BudgetIncomeItems = { new BudgetItem { Category = "Income", Amount = new Currency(1000) } },
                BudgetExpenseItems = { new BudgetExpenseCategory { CategoryName = "Expense" } },
                BudgetSavingsCategories = {
                    new BudgetSavingsCategory {
                        CategoryName = "Save",
                        BudgetedAmount = new Currency(200),
                        CurrentBalance = new Currency(500),
                        Transactions = { new Transaction(currentBudgetDate, "", new Category { Group = "Savings", Name = "Save" }, new Currency(500), "Initial") }
                    }
                }
            };

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns([currentBudget]);
            _mockDatabaseReader.Setup(x => x.GetCollection<Account>("Accounts"))
                .Returns([]);

            var dialogViewModel = new NewBudgetDialogViewModel
            {
                SelectedDate = nextBudgetTitle,
                UseLastMonthsBudget = true
            };

            _mockNewBudgetDialogService.Setup(x => x.GetViewModel())
                .Returns(dialogViewModel);
            _mockNewBudgetDialogService.Setup(x => x.ShowDialogAsync(_mockContentDialogService.Object))
                .ReturnsAsync(ContentDialogResult.Primary);

            _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockMessageBoxService.Object,
                _mockNewBudgetDialogService.Object,
                _mockBudgetCategoryDialogService.Object,
                _mockNewExpenseDialogService.Object,
                _mockSavingsCategoryDialogService.Object
            );

            _viewModel.OnPageNavigatedTo();
            _viewModel.CurrentBudget = _viewModel.Budgets[0];

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(2, _viewModel.Budgets.Count);
            var newBudget = _viewModel.Budgets[1];
            Assert.AreEqual(nextBudgetTitle, newBudget.BudgetTitle);
            Assert.AreEqual(1, newBudget.BudgetIncomeItems.Count);
            Assert.AreEqual("Income", newBudget.BudgetIncomeItems[0].Category);
            Assert.AreEqual(1, newBudget.BudgetExpenseItems.Count);
            Assert.AreEqual("Expense", newBudget.BudgetExpenseItems[0].CategoryName);
            Assert.AreEqual(1, newBudget.BudgetSavingsCategories.Count);

            var newSavings = newBudget.BudgetSavingsCategories[0];
            Assert.AreEqual("Save", newSavings.CategoryName);
            Assert.AreEqual(currentBudget.BudgetSavingsCategories[0].CurrentBalance + currentBudget.BudgetSavingsCategories[0].BudgetedAmount, newSavings.CurrentBalance);
            Assert.AreEqual(2, newSavings.Transactions.Count); // Old transactions are deleted during copy
            Assert.AreEqual(newSavings.BudgetedAmount, newSavings.Transactions[1].Amount);
            Assert.AreEqual(newSavings.PlannedTransactionHash, newSavings.Transactions[1].TransactionHash);
        }

        [TestMethod]
        public async Task CreateNewBudget_CopyFromLastMonth_NoBudgetsExist_DoesNotThrowAndCreatesEmptyBudget()
        {
            // Arrange
            var budgetDate = DateTime.Now.AddMonths(1);
            var budgetTitle = budgetDate.ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture);

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget>());

            var dialogViewModel = new NewBudgetDialogViewModel
            {
                SelectedDate = budgetTitle,
                UseLastMonthsBudget = true
            };

            _mockNewBudgetDialogService.Setup(x => x.GetViewModel())
                .Returns(dialogViewModel);
            _mockNewBudgetDialogService.Setup(x => x.ShowDialogAsync(_mockContentDialogService.Object))
                .ReturnsAsync(ContentDialogResult.Primary);

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(1, _viewModel.Budgets.Count);
            Assert.AreEqual(budgetTitle, _viewModel.Budgets[0].BudgetTitle);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetIncomeItems.Count);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetExpenseItems.Count);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetSavingsCategories.Count);
        }

        [TestMethod]
        public async Task CreateNewBudget_CopyFromLastMonth_CurrentBudgetNull_DoesNotThrowAndCreatesEmptyBudget()
        {
            // Arrange
            var budgetDate = DateTime.Now.AddMonths(1);
            var budgetTitle = budgetDate.ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture);

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget>());

            var dialogViewModel = new NewBudgetDialogViewModel
            {
                SelectedDate = budgetTitle,
                UseLastMonthsBudget = true
            };

            _mockNewBudgetDialogService.Setup(x => x.GetViewModel())
                .Returns(dialogViewModel);
            _mockNewBudgetDialogService.Setup(x => x.ShowDialogAsync(_mockContentDialogService.Object))
                .ReturnsAsync(ContentDialogResult.Primary);

            // Ensure CurrentBudget is null
            _viewModel.GetType().GetProperty("CurrentBudget")!.SetValue(_viewModel, null);

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(1, _viewModel.Budgets.Count);
            Assert.AreEqual(budgetTitle, _viewModel.Budgets[0].BudgetTitle);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetIncomeItems.Count);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetExpenseItems.Count);
            Assert.AreEqual(0, _viewModel.Budgets[0].BudgetSavingsCategories.Count);
        }
    }
}