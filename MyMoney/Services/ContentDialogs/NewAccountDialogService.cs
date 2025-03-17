using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Services.ContentDialogs
{
    public interface INewAccountDialogService
    {
        public void SetViewModel(NewAccountDialogViewModel viewModel);
        public NewAccountDialogViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService);
    }

    public class NewAccountDialogService : INewAccountDialogService
    {
        private NewAccountDialogViewModel _viewModel = new();

        public NewAccountDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(NewAccountDialogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            NewAccountDialog newAccountDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel"
            };
            return await dialogService.ShowAsync(newAccountDialog, CancellationToken.None);
        }
    }
}
