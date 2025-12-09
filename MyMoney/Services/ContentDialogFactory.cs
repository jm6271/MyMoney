using Wpf.Ui.Controls;

namespace MyMoney.Services
{
    public interface IContentDialogFactory
    {
        ContentDialog Create<T>() where T : ContentDialog, new();
    }

    public class ContentDialogFactory : IContentDialogFactory
    {
        public ContentDialog Create<T>()
            where T : ContentDialog, new()
        {
            var dialog = new T();

            return dialog;
        }
    }
}
