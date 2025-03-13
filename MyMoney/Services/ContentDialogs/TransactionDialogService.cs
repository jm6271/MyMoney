using MyMoney.ViewModels.ContentDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using Wpf.Ui;
using MyMoney.Views.ContentDialogs;

namespace MyMoney.Services.ContentDialogs
{
    public interface ITransactionDialogService
    {
        public void SetViewModel(NewTransactionDialogViewModel viewModel);
        public NewTransactionDialogViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService);
    }

    public class TransactionDialogService : ITransactionDialogService
    {
        NewTransactionDialogViewModel _viewModel = new();

        public NewTransactionDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(NewTransactionDialogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            NewTransactionDialog newAccountDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel"
            };
            return await dialogService.ShowAsync(newAccountDialog, CancellationToken.None);
        }
    }
}
