using MyMoney.ViewModels.Pages;
using Moq;
using MyMoney.Core.Models;
using MyMoney.Core.Database;
using MyMoney.Services.ContentDialogs;
using Wpf.Ui.Controls;
using Wpf.Ui;
using System.Collections.ObjectModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel
{
    [TestClass]
    public class DeleteExpenseGroupTests
    {
        private Mock<IContentDialogService> _mockContentDialogService;
        private Mock<IDatabaseReader> _mockDatabaseReader;
        private Mock<IMessageBoxService> _mockMessageBoxService;
        private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
        private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
        private Mock<INewExpenseGroupDialogService> _mockNewExpenseGroupDialogService;
        private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService;
        private ViewModels.Pages.BudgetViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockDatabaseReader = new Mock<IDatabaseReader>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
            _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
            _mockNewExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();
            _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();

            // Setup mock database with a test budget
            var testBudget = new Budget
            {
                BudgetDate = DateTime.Now,
                BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"),
            };

            _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(new List<Budget> { testBudget });

            _viewModel = new ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockMessageBoxService.Object,
                _mockNewBudgetDialogService.Object,
                _mockBudgetCategoryDialogService.Object,
                _mockNewExpenseGroupDialogService.Object,
                _mockSavingsCategoryDialogService.Object
            );
        }

        [TestMethod]
        public async Task DeleteExpenseGroup_SuccessfulDeletion_RemovesGroup()
        {
            // Arrange
            var expenseCategory = new BudgetExpenseCategory { CategoryName = "Test Category" };
            _viewModel.CurrentBudget?.BudgetExpenseItems.Add(expenseCategory);
            _mockMessageBoxService.Setup(x => x.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(MessageBoxResult.Primary);

            // Act
            await _viewModel.DeleteExpenseGroupCommand.ExecuteAsync(expenseCategory);

            // Assert
            Assert.IsNotNull(_viewModel.CurrentBudget);
            Assert.IsFalse(_viewModel.CurrentBudget.BudgetExpenseItems.Contains(expenseCategory));
        }

        [TestMethod]
        public async Task DeleteExpenseGroup_NullCurrentBudget_DoesNothing()
        {
            // Arrange
            _viewModel.CurrentBudget = null;
            var expenseCategory = new BudgetExpenseCategory { CategoryName = "Test Category" };

            // Act
            await _viewModel.DeleteExpenseGroupCommand.ExecuteAsync(expenseCategory);

            // Assert
            _mockMessageBoxService.Verify(x => x.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteExpenseGroup_EditingDisabled_DoesNothing()
        {
            // Arrange
            _viewModel.IsEditingEnabled = false;
            var expenseCategory = new BudgetExpenseCategory { CategoryName = "Test Category" };

            // Act
            await _viewModel.DeleteExpenseGroupCommand.ExecuteAsync(expenseCategory);

            // Assert
            _mockMessageBoxService.Verify(x => x.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteExpenseGroup_UserCancels_DoesNotDelete()
        {
            // Arrange
            var expenseCategory = new BudgetExpenseCategory { CategoryName = "Test Category" };
            _viewModel.CurrentBudget?.BudgetExpenseItems.Add(expenseCategory);
            _mockMessageBoxService.Setup(x => x.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(MessageBoxResult.Secondary);

            // Act
            await _viewModel.DeleteExpenseGroupCommand.ExecuteAsync(expenseCategory);

            // Assert
            Assert.IsNotNull(_viewModel.CurrentBudget);
            Assert.IsTrue(_viewModel.CurrentBudget.BudgetExpenseItems.Contains(expenseCategory));
        }
    }
}