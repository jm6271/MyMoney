using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;
using Wpf.Ui;
using MyMoney.Views.ContentDialogs;

namespace MyMoney.Services.ContentDialogs
{
    public interface IRenameAccountDialogService
    {
        public void SetViewModel(RenameAccountViewModel viewModel);
        public RenameAccountViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService);
    }
    public class RenameAccountDialogService : IRenameAccountDialogService
    {
        private RenameAccountViewModel _viewModel = new();
        public RenameAccountViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(RenameAccountViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            RenameAccountDialog renameAccountDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel"
            };
            return await dialogService.ShowAsync(renameAccountDialog, CancellationToken.None);
        }
    }
}
