using MyMoney.Abstractions;
using Wpf.Ui.Controls;

namespace MyMoney.Services
{
    public interface IContentDialogFactory
    {
        IContentDialog Create<T>() where T : IContentDialog, new();
    }

    public class ContentDialogFactory : IContentDialogFactory
    {
        public IContentDialog Create<T>()
            where T : IContentDialog, new()
        {
            var dialog = new T();

            return dialog;
        }
    }
}
