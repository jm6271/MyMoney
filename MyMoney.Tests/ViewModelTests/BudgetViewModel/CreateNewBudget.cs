using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel
{
    [TestClass]
    public class CreateNewBudgetTests
    {
        private Mock<IContentDialogService> _mockContentDialogService = null!;
        private Mock<IDatabaseManager> _mockDatabaseReader = null!;
        private Mock<IMessageBoxService> _mockMessageBoxService = null!;
        private Mock<IContentDialogFactory> _mockContentDialogFactory = null!;
        private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockDatabaseReader = new Mock<IDatabaseManager>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockContentDialogFactory = new Mock<IContentDialogFactory>();

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

            _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockMessageBoxService.Object,
                _mockContentDialogFactory.Object
            );
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenBudgetDoesntExist_CreatesNewBudget()
        {
            // Arrange
            var budgetDate = DateTime.Now.AddMonths(1);
            var budgetTitle = budgetDate.ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var fakeDialog = new Mock<IContentDialog>();
            fakeDialog.SetupAllProperties();
            fakeDialog
                .Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fakeDialog.Object.DataContext as NewBudgetDialogViewModel;
                        vm!.SelectedDate = budgetTitle;
                        vm.UseLastMonthsBudget = false;
                    }
                )
                .ReturnsAsync(ContentDialogResult.Primary);

            _mockContentDialogFactory.Setup(x => x.Create<NewBudgetDialog>()).Returns(fakeDialog.Object);

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.HasCount(1, _viewModel.Budgets);
            Assert.AreEqual(budgetTitle, _viewModel.Budgets[0].BudgetTitle);
            Assert.HasCount(0, _viewModel.Budgets[0].BudgetIncomeItems);
            Assert.HasCount(0, _viewModel.Budgets[0].BudgetExpenseItems);
            Assert.HasCount(0, _viewModel.Budgets[0].BudgetSavingsCategories);
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenBudgetExistsAlready_ShowsWarningAndDoesNotCreate()
        {
            // Arrange
            var existingBudget = new Budget
            {
                BudgetTitle = DateTime
                    .Now.AddMonths(1)
                    .ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture),
                BudgetDate = DateTime.Now.AddMonths(1),
            };

            _mockDatabaseReader
                .Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget> { existingBudget });

            var fakeDialog = new Mock<IContentDialog>();
            fakeDialog.SetupAllProperties();
            fakeDialog
                .Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fakeDialog.Object.DataContext as NewBudgetDialogViewModel;
                        vm!.SelectedDate = existingBudget.BudgetTitle;
                        vm.UseLastMonthsBudget = false;
                    }
                )
                .ReturnsAsync(ContentDialogResult.Primary);

            _mockContentDialogFactory.Setup(x => x.Create<NewBudgetDialog>()).Returns(fakeDialog.Object);

            await _viewModel.OnNavigatedToAsync();
            _viewModel.CurrentBudget = _viewModel.Budgets[0];

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.HasCount(1, _viewModel.Budgets); // Only the existing budget
            _mockMessageBoxService.Verify(
                x =>
                    x.ShowInfoAsync(
                        "Budget Already Exists",
                        "Cannot create a budget for the selected month because a budget for this month already exists",
                        "OK"
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenDialogResultNotPrimary_DoesNotCreateBudget()
        {
            // Arrange
            var fakeDialog = new Mock<IContentDialog>();
            fakeDialog.SetupAllProperties();
            fakeDialog
                .Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ContentDialogResult.Secondary);

            _mockContentDialogFactory.Setup(x => x.Create<NewBudgetDialog>()).Returns(fakeDialog.Object);

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.HasCount(0, _viewModel.Budgets);
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenCopyingFromCurrentBudget_CopiesAllItems()
        {
            // Arrange
            var currentBudgetDate = DateTime.Now;
            var nextBudgetDate = currentBudgetDate.AddMonths(1);
            var currentBudgetTitle = currentBudgetDate.ToString(
                "MMMM, yyyy",
                System.Globalization.CultureInfo.InvariantCulture
            );
            var nextBudgetTitle = nextBudgetDate.ToString(
                "MMMM, yyyy",
                System.Globalization.CultureInfo.InvariantCulture
            );

            var currentBudget = new Budget
            {
                BudgetTitle = currentBudgetTitle,
                BudgetDate = currentBudgetDate,
                BudgetIncomeItems =
                {
                    new BudgetItem { Category = "Income", Amount = new Currency(1000) },
                },
                BudgetExpenseItems = { new BudgetExpenseCategory { CategoryName = "Expense" } },
                BudgetSavingsCategories =
                {
                    new BudgetSavingsCategory
                    {
                        CategoryName = "Save",
                        BudgetedAmount = new Currency(200),
                        CurrentBalance = new Currency(500),
                        Transactions =
                        {
                            new Transaction(
                                currentBudgetDate,
                                "",
                                new Category { Group = "Savings", Name = "Save" },
                                new Currency(500),
                                "Initial"
                            ),
                        },
                    },
                },
            };

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns([currentBudget]);
            _mockDatabaseReader.Setup(x => x.GetCollection<Account>("Accounts")).Returns([]);

            var fakeDialog = new Mock<IContentDialog>();
            fakeDialog.SetupAllProperties();
            fakeDialog
                .Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(
                    (ct) =>
                    {
                        var vm = fakeDialog.Object.DataContext as NewBudgetDialogViewModel;
                        vm!.SelectedDate = nextBudgetTitle;
                        vm.UseLastMonthsBudget = true;
                    }
                )
                .ReturnsAsync(ContentDialogResult.Primary);

            _mockContentDialogFactory.Setup(x => x.Create<NewBudgetDialog>()).Returns(fakeDialog.Object);

            await _viewModel.OnNavigatedToAsync();
            _viewModel.CurrentBudget = _viewModel.Budgets[0];

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.HasCount(2, _viewModel.Budgets);
            var newBudget = _viewModel.Budgets[1];
            Assert.AreEqual(nextBudgetTitle, newBudget.BudgetTitle);
            Assert.HasCount(1, newBudget.BudgetIncomeItems);
            Assert.AreEqual("Income", newBudget.BudgetIncomeItems[0].Category);
            Assert.HasCount(1, newBudget.BudgetExpenseItems);
            Assert.AreEqual("Expense", newBudget.BudgetExpenseItems[0].CategoryName);
            Assert.HasCount(1, newBudget.BudgetSavingsCategories);

            var newSavings = newBudget.BudgetSavingsCategories[0];
            Assert.AreEqual("Save", newSavings.CategoryName);
            Assert.AreEqual(
                currentBudget.BudgetSavingsCategories[0].CurrentBalance
                    + currentBudget.BudgetSavingsCategories[0].BudgetedAmount,
                newSavings.CurrentBalance
            );
            Assert.HasCount(2, newSavings.Transactions); // Old transactions are deleted during copy
            Assert.AreEqual(newSavings.BudgetedAmount, newSavings.Transactions[1].Amount);
            Assert.AreEqual(newSavings.PlannedTransactionHash, newSavings.Transactions[1].TransactionHash);
        }
    }
}
