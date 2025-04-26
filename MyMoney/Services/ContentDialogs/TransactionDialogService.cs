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
        public void SetTitle(string title);
        public string GetSelectedPayee();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService);
    }

    public class TransactionDialogService : ITransactionDialogService
    {
        NewTransactionDialogViewModel _viewModel = new();
        string _title = "New Transaction";
        string _selectedPayee = "";

        public string GetSelectedPayee()
        {
            return _selectedPayee;
        }

        public NewTransactionDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetTitle(string title)
        {
            _title = title;
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

            NewTransactionDialog newTransactionDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                Title = _title
            };
            var result =  await dialogService.ShowAsync(newTransactionDialog, CancellationToken.None);
            if (result != ContentDialogResult.Primary) return result;

            _selectedPayee = newTransactionDialog.SelectedPayee;
            _viewModel.NewTransactionCategory = newTransactionDialog.SelectedCategory;
            return result;
        }
    }
}
