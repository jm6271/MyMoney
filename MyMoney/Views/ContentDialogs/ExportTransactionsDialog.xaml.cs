using MyMoney.Abstractions;
using MyMoney.ViewModels.ContentDialogs;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs;

public partial class ExportTransactionsDialog : ContentDialog, IContentDialog
{
    public ExportTransactionsDialog()
    {
        InitializeComponent();
    }

    private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        if (args.Result != ContentDialogResult.Primary)
            return;

        if (DataContext is not ExportTransactionsDialogViewModel viewModel)
            return;

        viewModel.Validate();

        if (viewModel.HasErrors)
        {
            args.Cancel = true;
        }
    }
}
