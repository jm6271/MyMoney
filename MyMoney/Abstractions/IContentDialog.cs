using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace MyMoney.Abstractions
{
    public interface IContentDialog
    {
        public object? DataContext { get; set; }

        public object? Title { get; set; }

        public string PrimaryButtonText { get; set; }
        public string SecondaryButtonText { get; set; }
        public string CloseButtonText { get; set; }

        public ContentDialogHost? DialogHostEx { get; set; }

        public Task<ContentDialogResult> ShowAsync(CancellationToken cancellationToken = default);
    }
}
