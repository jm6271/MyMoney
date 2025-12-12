using System.Collections.ObjectModel;
using Moq;
using MyMoney.Abstractions;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[TestClass]
public class AddExpenseGroupTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<Core.Database.IDatabaseManager> _mockDatabaseReader;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockDatabaseReader = new Mock<Core.Database.IDatabaseManager>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());
    }

    [TestMethod]
    public async Task AddExpenseGroup_WhenSuccessful_AddsNewGroupToBudget()
    {
        // Arrange
        var viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );

        viewModel.CurrentBudget = new Budget { BudgetExpenseItems = new ObservableCollection<BudgetExpenseCategory>() };

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewExpenseGroupDialogViewModel;
                    vm?.GroupName = "Test Group";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<NewExpenseGroupDialog>()).Returns(fake.Object);

        // Act
        await viewModel.AddExpenseGroupCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, viewModel.CurrentBudget.BudgetExpenseItems);
        Assert.AreEqual("Test Group", viewModel.CurrentBudget.BudgetExpenseItems[0].CategoryName);
    }

    [TestMethod]
    public async Task AddExpenseGroup_GroupAlreadyExists_ShowMessage()
    {
        // Arrange
        var viewModel = new MyMoney.ViewModels.Pages.BudgetViewModel(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );

        viewModel.CurrentBudget = new Budget { BudgetExpenseItems = [new() { CategoryName = "Test Group" }] };

        var fake = new Mock<IContentDialog>();
        fake.SetupAllProperties();
        fake.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                (ct) =>
                {
                    var vm = fake.Object.DataContext as NewExpenseGroupDialogViewModel;
                    vm?.GroupName = "Test Group";
                }
            )
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<NewExpenseGroupDialog>()).Returns(fake.Object);

        // Act
        await viewModel.AddExpenseGroupCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, viewModel.CurrentBudget.BudgetExpenseItems);
        _mockMessageBoxService.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }
}
