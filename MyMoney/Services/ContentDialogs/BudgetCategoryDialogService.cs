using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;
using Wpf.Ui;
using MyMoney.Views.ContentDialogs;

namespace MyMoney.Services.ContentDialogs
{
    public interface IBudgetCategoryDialogService
    {
        public void SetViewModel(BudgetCategoryDialogViewModel viewModel);
        public BudgetCategoryDialogViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService, string title);
    }

    public class BudgetCategoryDialogService : IBudgetCategoryDialogService
    {
        private BudgetCategoryDialogViewModel _viewModel = new();

        public BudgetCategoryDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(BudgetCategoryDialogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService, string title)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            BudgetCategoryDialog budgetCategoryDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                Title = title
            };
            return await dialogService.ShowAsync(budgetCategoryDialog, CancellationToken.None);
        }
    }
}
