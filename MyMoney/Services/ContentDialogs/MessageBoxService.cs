using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Services.ContentDialogs
{
    public interface IMessageBoxService
    {
        public Task<Wpf.Ui.Controls.MessageBoxResult> ShowAsync(string title, string content, string primaryButtonText,
                                                                string closeButtonText);
    }

    public class MessageBoxService : IMessageBoxService
    {
        public async Task<Wpf.Ui.Controls.MessageBoxResult> ShowAsync(string title, string content, string primaryButtonText, string closeButtonText)
        {
            Wpf.Ui.Controls.MessageBox messageBox = new()
            {
                Title = title,
                Content = content,
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText
            };
            return await messageBox.ShowDialogAsync();
        }
    }
}
