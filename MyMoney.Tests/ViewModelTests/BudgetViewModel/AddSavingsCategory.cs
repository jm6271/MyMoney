using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MyMoney.Core.Database;
using MyMoney.Core.Models;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using MyMoney.Views.ContentDialogs;
using MyMoney.Abstractions;
using Wpf.Ui;
using Wpf.Ui.Controls;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace MyMoney.Tests.ViewModelTests.BudgetViewModel;

[TestClass]
public class AddSavingsCategoryTests
{
    private Mock<IContentDialogService> _mockContentDialogService;
    private Mock<IDatabaseManager> _mockDatabaseReader;
    private Mock<IMessageBoxService> _mockMessageBoxService;
    private Mock<IContentDialogFactory> _mockContentDialogFactory;
    private MyMoney.ViewModels.Pages.BudgetViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockContentDialogService = new Mock<IContentDialogService>();
        _mockDatabaseReader = new Mock<IDatabaseManager>();
        _mockMessageBoxService = new Mock<IMessageBoxService>();
        _mockContentDialogFactory = new Mock<IContentDialogFactory>();

        // Setup database reader to return empty collection
        _mockDatabaseReader.Setup(x => x.GetCollection<Budget>("Budgets")).Returns(new List<Budget>());

        _viewModel = new(
            _mockContentDialogService.Object,
            _mockDatabaseReader.Object,
            _mockMessageBoxService.Object,
            _mockContentDialogFactory.Object
        );
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenDialogCancelled_ShouldNotAddCategory()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var fakeDialog = new Mock<IContentDialog>();
        fakeDialog.SetupAllProperties();
        fakeDialog.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ContentDialogResult.Secondary);

        _mockContentDialogFactory.Setup(x => x.Create<SavingsCategoryDialog>()).Returns(fakeDialog.Object);

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.IsEmpty(_viewModel.CurrentBudget.BudgetSavingsCategories);
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenDialogConfirmed_ShouldAddNewCategory()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        var fakeDialog = new Mock<IContentDialog>();
        fakeDialog.SetupAllProperties();
        fakeDialog.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>((ct) =>
            {
                var vm = fakeDialog.Object.DataContext as SavingsCategoryDialogViewModel;
                vm!.Category = "Test Savings";
                vm.Planned = new Currency(200m);
            })
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<SavingsCategoryDialog>()).Returns(fakeDialog.Object);

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        Assert.HasCount(1, _viewModel.CurrentBudget.BudgetSavingsCategories);
        Assert.AreEqual("Test Savings", _viewModel.CurrentBudget.BudgetSavingsCategories[0].CategoryName);
        Assert.AreEqual(200m, _viewModel.CurrentBudget.BudgetSavingsCategories[0].BudgetedAmount.Value);
    }

    [TestMethod]
    public async Task AddSavingsCategory_WhenSavingsCategoryAlreadyExists_ShouldShowErrorMessage()
    {
        // Arrange
        _viewModel.CurrentBudget = new Budget();
        _viewModel.CurrentBudget.BudgetSavingsCategories.Add(
            new BudgetSavingsCategory { CategoryName = "Existing Savings", BudgetedAmount = new Currency(100m) }
        );

        var fakeDialog = new Mock<IContentDialog>();
        fakeDialog.SetupAllProperties();
        fakeDialog.Setup(x => x.ShowAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>((ct) =>
            {
                var vm = fakeDialog.Object.DataContext as SavingsCategoryDialogViewModel;
                vm!.Category = "Existing Savings";
                vm.Planned = new Currency(200m);
            })
            .ReturnsAsync(ContentDialogResult.Primary);

        _mockContentDialogFactory.Setup(x => x.Create<SavingsCategoryDialog>()).Returns(fakeDialog.Object);

        // Act
        await _viewModel.AddSavingsCategoryCommand.ExecuteAsync(null);

        // Assert
        _mockMessageBoxService.Verify(
            x => x.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
        Assert.HasCount(1, _viewModel.CurrentBudget.BudgetSavingsCategories);
    }
}
