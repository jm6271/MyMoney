using MyMoney.Core.FS.Models;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Moq;
using MyMoney.Views.ContentDialogs;
using MyMoney.Services.ContentDialogs;

namespace MyMoney.Tests.ViewModelTests;

[STATestClass]
public class AccountsViewModelTest
{
    [STATestMethod]
    public void Test_NewAccount()
    {
        // The view model the dialog returns
        NewAccountDialogViewModel newAccountDialogViewModel = new()
        {
            AccountName = "Savings",
            StartingBalance = new Currency(500m)
        };

        // Create a mock database object
        var mockDatabaseService = new Mock<Core.Database.IDatabaseReader>();
        mockDatabaseService.Setup(service => service.GetCollection<Account>("Accounts")).Returns([]);

        // Create a mock content dialog service
        var mockContentDialogService = new Mock<IContentDialogService>();
        mockContentDialogService.Setup(service => service.GetDialogHost()).Returns(new System.Windows.Controls.ContentPresenter());
        mockContentDialogService.Setup(service => service.ShowAsync(It.IsAny<NewAccountDialog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Wpf.Ui.Controls.ContentDialogResult.Primary);

        // Create a mock new account dialog service
        var mockNewAccountDialogService = new Mock<INewAccountDialogService>();
        mockNewAccountDialogService.Setup(service => service.ShowDialogAsync(It.IsAny<IContentDialogService>())).ReturnsAsync(
            Wpf.Ui.Controls.ContentDialogResult.Primary);
        mockNewAccountDialogService.Setup(service => service.GetViewModel()).Returns(newAccountDialogViewModel);

        AccountsViewModel viewModel = new(mockContentDialogService.Object, mockDatabaseService.Object, mockNewAccountDialogService.Object);
        viewModel.CreateNewAccountCommand.Execute(null);

        Assert.AreEqual(1, viewModel.Accounts.Count);
        Assert.AreEqual("Savings", viewModel.Accounts[0].AccountName);
        Assert.AreEqual(500m, viewModel.Accounts[0].Total.Value);
    }
}
