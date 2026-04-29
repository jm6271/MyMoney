using System.Windows;
using System.Windows.Media;
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

        var hasError = false;
        ValidationText.Visibility = Visibility.Collapsed;
        FieldsBorder.BorderBrush = Brushes.Transparent;

        if (!viewModel.HasSelectedFields)
        {
            hasError = true;
            FieldsBorder.BorderBrush = Brushes.Red;
            ValidationText.Text = "Select at least one field.";
            ValidationText.Visibility = Visibility.Visible;
        }
        else if (viewModel.UseDateRange && viewModel.StartDate.Date > viewModel.EndDate.Date)
        {
            hasError = true;
            ValidationText.Text = "Start date must be on or before end date.";
            ValidationText.Visibility = Visibility.Visible;
        }

        args.Cancel = hasError;
    }
}
