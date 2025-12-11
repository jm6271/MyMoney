using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel
{
    [TestClass]
    public class EditExpenseGroupTests
    {
        private Mock<IContentDialogService> _mockContentDialogService;
        private Mock<IMessageBoxService> _mockMessageBoxService;
        private Mock<INewBudgetDialogService> _mockNewBudgetDialogService;
        private Mock<IBudgetCategoryDialogService> _mockBudgetCategoryDialogService;
        private Mock<INewExpenseGroupDialogService> _mockExpenseGroupDialogService;
        private Mock<ISavingsCategoryDialogService> _mockSavingsCategoryDialogService;
        private Mock<IContentDialogFactory> _mockContentDialogFactory;
        private Mock<IDatabaseManager> _mockDatabaseReader;
        private ViewModels.Pages.BudgetViewModel _viewModel;

        [TestInitialize]
        public async Task Setup()
        {
            _mockContentDialogService = new Mock<IContentDialogService>();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockNewBudgetDialogService = new Mock<INewBudgetDialogService>();
            _mockBudgetCategoryDialogService = new Mock<IBudgetCategoryDialogService>();
            _mockExpenseGroupDialogService = new Mock<INewExpenseGroupDialogService>();
            _mockSavingsCategoryDialogService = new Mock<ISavingsCategoryDialogService>();
            _mockContentDialogFactory = new Mock<IContentDialogFactory>();
            _mockDatabaseReader = new Mock<IDatabaseManager>();

            _mockDatabaseReader
                .Setup(x => x.GetCollection<Budget>("Budgets"))
                .Returns(
                    new List<Budget>
                    {
                        new Budget { BudgetTitle = DateTime.Now.ToString("MMMM, yyyy"), BudgetDate = DateTime.Now },
                    }
                );

            _viewModel = new ViewModels.Pages.BudgetViewModel(
                _mockContentDialogService.Object,
                _mockDatabaseReader.Object,
                _mockMessageBoxService.Object,
                _mockNewBudgetDialogService.Object,
                _mockBudgetCategoryDialogService.Object,
                _mockExpenseGroupDialogService.Object,
                _mockSavingsCategoryDialogService.Object,
                _mockContentDialogFactory.Object
            );

            await _viewModel.OnNavigatedToAsync();
            _viewModel.CurrentBudget = _viewModel.Budgets[0];
        }

        [TestMethod]
        public async Task EditExpenseGroup_WhenDialogReturnsPrimary_UpdatesGroupName()
        {
            // Arrange
            var parameter = new BudgetExpenseCategory { CategoryName = "Old Name" };
            var newName = "New Group Name";

            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    var vm = fake.Object.DataContext as NewExpenseGroupDialogViewModel;
                    vm?.GroupName = newName;
                })
                .ReturnsAsync(ContentDialogResult.Primary);

            _mockContentDialogFactory.Setup(x => x.Create<NewExpenseGroupDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.EditExpenseGroupCommand.ExecuteAsync(parameter);

            // Assert
            Assert.AreEqual(newName, parameter.CategoryName);
        }

        [TestMethod]
        public async Task EditExpenseGroup_WhenDialogReturnsSecondary_DoesNotUpdateGroupName()
        {
            // Arrange
            var originalName = "Original Name";
            var parameter = new BudgetExpenseCategory { CategoryName = originalName };

            var fake = new Mock<IContentDialog>();
            fake.SetupAllProperties();
            fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    var vm = fake.Object.DataContext as NewExpenseGroupDialogViewModel;
                    vm?.GroupName = "Test Group";
                })
                .ReturnsAsync(ContentDialogResult.Secondary);

            _mockContentDialogFactory.Setup(x => x.Create<NewExpenseGroupDialog>()).Returns(fake.Object);

            // Act
            await _viewModel.EditExpenseGroupCommand.ExecuteAsync(parameter);

            // Assert
            Assert.AreEqual(originalName, parameter.CategoryName);
        }

        [TestMethod]
        public async Task EditExpenseGroup_WhenCurrentBudgetIsNull_ReturnsEarly()
        {
            // Arrange
            _viewModel.CurrentBudget = null;
            var parameter = new BudgetExpenseCategory { CategoryName = "Test" };

            // Act
            await _viewModel.EditExpenseGroupCommand.ExecuteAsync(parameter);

            // Assert
            _mockExpenseGroupDialogService.Verify(
                x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [TestMethod]
        public async Task EditExpenseGroup_WhenEditingIsDisabled_ReturnsEarly()
        {
            // Arrange
            _viewModel.IsEditingEnabled = false;
            var parameter = new BudgetExpenseCategory { CategoryName = "Test" };

            // Act
            await _viewModel.EditExpenseGroupCommand.ExecuteAsync(parameter);

            // Assert
            _mockExpenseGroupDialogService.Verify(
                x => x.ShowDialogAsync(It.IsAny<IContentDialogService>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }
    }
}
