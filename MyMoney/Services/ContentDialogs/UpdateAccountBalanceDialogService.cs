using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Services.ContentDialogs
{
    public interface IUpdateAccountBalanceDialogService
    {
        public void SetViewModel(UpdateAccountBalanceDialogViewModel viewModel);
        public UpdateAccountBalanceDialogViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService);
    }

    public class UpdateAccountBalanceDialogService : IUpdateAccountBalanceDialogService
    {
        UpdateAccountBalanceDialogViewModel _viewModel = new();

        public UpdateAccountBalanceDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(UpdateAccountBalanceDialogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            UpdateAccountBalanceDialog updateBalanceDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "Update",
                CloseButtonText = "Cancel"
            };
            return await dialogService.ShowAsync(updateBalanceDialog, CancellationToken.None);
        }
    }
}
