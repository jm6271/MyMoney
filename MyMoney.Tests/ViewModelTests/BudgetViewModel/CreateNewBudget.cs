using Moq;
using MyMoney.Core.Database;
using MyMoney.Services;
using Wpf.Ui;

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel
{
    [TestClass]
    public class CreateNewBudgetTests
    {
        private Mock<IContentDialogService> _mockContentDialogService = null!;
        private Mock<IMessageBoxService> _mockMessageBoxService = null!;
        private Mock<IContentDialogFactory> _mockContentDialogFactory = null!;
        private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel = null!;

        private DatabaseManager _databaseManager = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockContentDialogFactory = new Mock<IContentDialogFactory>();

            _databaseManager = new(new MemoryStream());

            _viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _databaseManager,
                _mockMessageBoxService.Object,
                _mockContentDialogFactory.Object
            );
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenBudgetDoesntExist_CreatesNewBudget()
        {
            // Arrange: set a target month with no prior budget in the database
            var targetMonth = new DateTime(2025, 6, 1);
            _viewModel.SelectedBudgetMonth = targetMonth;

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert: one budget inserted into the "Budgets" collection
            var budgets = _databaseManager.GetCollection<MyMoney.Core.Models.Budget>("Budgets");
            Assert.AreEqual(1, budgets.Count);

            var newBudget = budgets[0];

            // Assert: title derived from SelectedBudgetMonth using "MMMM, yyyy" format
            Assert.AreEqual(targetMonth.ToString("MMMM, yyyy"), newBudget.BudgetTitle);

            // Assert: empty income, expense, and savings collections
            Assert.AreEqual(0, newBudget.BudgetIncomeItems.Count);
            Assert.AreEqual(0, newBudget.BudgetExpenseItems.Count);
            Assert.AreEqual(0, newBudget.BudgetSavingsCategories.Count);
        }

        [TestMethod]
        public async Task CreateNewBudget_WhenCopyingFromCurrentBudget_CopiesAllItems()
        {
            // Arrange: build a prior budget for May 2025
            var priorMonth = new DateTime(2025, 5, 1);
            var targetMonth = new DateTime(2025, 6, 1);

            var priorBudget = new MyMoney.Core.Models.Budget
            {
                BudgetTitle = priorMonth.ToString("MMMM, yyyy"),
                BudgetDate = priorMonth,
            };

            // Add an income item
            priorBudget.BudgetIncomeItems.Add(new MyMoney.Core.Models.BudgetItem
            {
                Category = "Salary",
                Amount = new MyMoney.Core.Models.Currency(3000m),
            });

            // Add an expense group with one sub-item
            var expenseGroup = new MyMoney.Core.Models.BudgetExpenseCategory { CategoryName = "Housing" };
            expenseGroup.SubItems.Add(new MyMoney.Core.Models.BudgetItem
            {
                Category = "Rent",
                Amount = new MyMoney.Core.Models.Currency(1200m),
            });
            priorBudget.BudgetExpenseItems.Add(expenseGroup);

            // Add a savings category with an existing transaction, a balance, and a budgeted amount
            var savingsCategory = new MyMoney.Core.Models.BudgetSavingsCategory
            {
                CategoryName = "Emergency Fund",
                BudgetedAmount = new MyMoney.Core.Models.Currency(200m),
                CurrentBalance = new MyMoney.Core.Models.Currency(500m),
            };
            savingsCategory.Transactions.Add(new MyMoney.Core.Models.Transaction(
                priorMonth,
                "",
                new MyMoney.Core.Models.Category { Group = "Savings", Name = "Emergency Fund" },
                new MyMoney.Core.Models.Currency(500m),
                "Initial deposit"
            ));
            priorBudget.BudgetSavingsCategories.Add(savingsCategory);

            _databaseManager.Insert("Budgets", priorBudget);

            // Load the prior budget into CurrentBudget by navigating to that month
            _viewModel.SelectedBudgetMonth = priorMonth;
            await _viewModel.BudgetMonthChangedCommand.ExecuteAsync(null);

            // Set target month for the new budget
            _viewModel.SelectedBudgetMonth = targetMonth;

            // Act
            await _viewModel.CreateNewBudgetCommand.ExecuteAsync(null);

            // Assert: two budgets in the database (prior + new)
            var budgets = _databaseManager.GetCollection<MyMoney.Core.Models.Budget>("Budgets");
            Assert.AreEqual(2, budgets.Count);

            // Find the new budget (June 2025)
            var newBudget = budgets.First(b => b.BudgetDate.Month == targetMonth.Month && b.BudgetDate.Year == targetMonth.Year);

            // Assert income items are copied
            Assert.AreEqual(1, newBudget.BudgetIncomeItems.Count);
            Assert.AreEqual("Salary", newBudget.BudgetIncomeItems[0].Category);
            Assert.AreEqual(3000m, newBudget.BudgetIncomeItems[0].Amount.Value);

            // Assert expense groups are copied
            Assert.AreEqual(1, newBudget.BudgetExpenseItems.Count);
            Assert.AreEqual("Housing", newBudget.BudgetExpenseItems[0].CategoryName);

            // Assert savings categories are copied
            Assert.AreEqual(1, newBudget.BudgetSavingsCategories.Count);
            var copiedSavings = newBudget.BudgetSavingsCategories[0];
            Assert.AreEqual("Emergency Fund", copiedSavings.CategoryName);

            // Assert exactly 2 transactions: balance-carried-forward and planned-this-month
            Assert.AreEqual(2, copiedSavings.Transactions.Count);

            // Assert CurrentBalance = prior balance (500) + budgeted amount (200) = 700
            Assert.AreEqual(500m + 200m, copiedSavings.CurrentBalance.Value);

            // Assert PlannedTransactionHash matches the planned-this-month transaction
            var plannedTransaction = copiedSavings.Transactions.First(t => t.TransactionDetail == "Planned This Month");
            Assert.AreEqual(plannedTransaction.TransactionHash, copiedSavings.PlannedTransactionHash);
        }
    }
}
