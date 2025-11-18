using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMoney.ViewModels.ContentDialogs;
using MyMoney.Views.ContentDialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyMoney.Services.ContentDialogs
{
    public interface ISavingsCategoryDialogService
    {
        public void SetViewModel(SavingsCategoryDialogViewModel viewModel);
        public SavingsCategoryDialogViewModel GetViewModel();
        public Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService, string title);
    }

    public class SavingsCategoryDialogService : ISavingsCategoryDialogService
    {
        private SavingsCategoryDialogViewModel _viewModel = new();

        public SavingsCategoryDialogViewModel GetViewModel()
        {
            return _viewModel;
        }

        public void SetViewModel(SavingsCategoryDialogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task<ContentDialogResult> ShowDialogAsync(IContentDialogService dialogService, string title)
        {
            var host = dialogService.GetDialogHost();
            if (host == null)
                return ContentDialogResult.None;

            SavingsCategoryDialog dialog = new(host, _viewModel)
            {
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                Title = title,
            };
            return await dialogService.ShowAsync(dialog, CancellationToken.None);
        }
    }
}
