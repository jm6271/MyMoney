using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Services.ContentDialogs
{
    public interface ITransferDialogService
    {
        public void SetViewModel(TransferDialogViewModel viewModel);
        public TransferDialogViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService);
    }

    public class TransferDialogService : ITransferDialogService
    {
        private TransferDialogViewModel _viewModel = new([]);

        public TransferDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(TransferDialogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            TransferDialog transferDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
            };
            return await dialogService.ShowAsync(transferDialog, CancellationToken.None);
        }
    }
}
