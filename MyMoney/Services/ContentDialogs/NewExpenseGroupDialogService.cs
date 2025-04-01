using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using Wpf.Ui;

namespace MyMoney.Services.ContentDialogs
{
    public interface INewExpenseGroupDialogService
    {
        public void SetViewModel(NewExpenseGroupDialogViewModel viewModel);
        public NewExpenseGroupDialogViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService);
    }
    public class NewExpenseGroupDialogService : INewExpenseGroupDialogService
    {
        private NewExpenseGroupDialogViewModel _viewModel = new();
        public NewExpenseGroupDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(NewExpenseGroupDialogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            NewExpenseGroupDialog newExpenseGroupDialog = new(host, _viewModel)
            {
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel"
            };
            return await dialogService.ShowAsync(newExpenseGroupDialog, CancellationToken.None);
        }
    }
}
