using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using System.Collections.ObjectModel;
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
        private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockDatabaseReader = new Mock<IDatabaseReader>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
            _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget>());

            _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockMessageBoxService.Object,
                _mockNewBudgetDialogService.Object,
                _mockBudgetCategoryDialogService.Object
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
            Assert.AreEqual(1, _viewModel.FutureBudgets.Count);
            Assert.AreEqual(budgetTitle, _viewModel.FutureBudgets[0].BudgetTitle);
            Assert.AreEqual(0, _viewModel.FutureBudgets[0].BudgetIncomeItems.Count);
            Assert.AreEqual(0, _viewModel.FutureBudgets[0].BudgetExpenseItems.Count);
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
                _mockBudgetCategoryDialogService.Object
            );

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            _mockMessageBoxService.Verify(x => x.ShowInfoAsync(
                "Budget Already Exists",
                "Cannot create a budget for the selected month because a budget for this month already exists",
                "OK"
            ), Times.Once);
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenCopyingFromCurrentBudget_CopiesAllItems()
        {
            // Arrange
            var currentBudget = new Budget
            {
                BudgetTitle = DateTime.Now.ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture),
                BudgetDate = DateTime.Now,
                BudgetIncomeItems = { new BudgetItem { Category = "Income", Amount = new Currency(1000) } },
                BudgetExpenseItems = { new BudgetItem { Category = "Expense", Amount = new Currency(500) } }
            };

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget> { currentBudget });

            var dialogViewModel = new NewBudgetDialogViewModel
            {
                SelectedDate = DateTime.Now.AddMonths(1).ToString("MMMM, yyyy", System.Globalization.CultureInfo.InvariantCulture),
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
                _mockBudgetCategoryDialogService.Object
            );

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(1, _viewModel.FutureBudgets.Count);
            Assert.AreEqual(1, _viewModel.FutureBudgets[0].BudgetIncomeItems.Count);
            Assert.AreEqual(1, _viewModel.FutureBudgets[0].BudgetExpenseItems.Count);
            Assert.AreEqual("Income", _viewModel.FutureBudgets[0].BudgetIncomeItems[0].Category);
            Assert.AreEqual("Expense", _viewModel.FutureBudgets[0].BudgetExpenseItems[0].Category);
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
            Assert.AreEqual(0, _viewModel.FutureBudgets.Count);
        }
    }
}