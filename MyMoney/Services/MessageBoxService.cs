using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Services
{
    public interface IMessageBoxService
    {
        public Task<Wpf.Ui.Controls.MessageBoxResult> ShowAsync(
            string title,
            string content,
            string primaryButtonText,
            string closeButtonText
        );
        public Task<Wpf.Ui.Controls.MessageBoxResult> ShowInfoAsync(
            string title,
            string content,
            string closeButtonText
        );
    }

    public class MessageBoxService : IMessageBoxService
    {
        public async Task<Wpf.Ui.Controls.MessageBoxResult> ShowAsync(
            string title,
            string content,
            string primaryButtonText,
            string closeButtonText
        )
        {
            Wpf.Ui.Controls.MessageBox messageBox = new()
            {
                Title = title,
                Content = content,
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = primaryButtonText,
                PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Secondary,
                CloseButtonText = closeButtonText,
                CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary,
            };
            return await messageBox.ShowDialogAsync();
        }

        public async Task<Wpf.Ui.Controls.MessageBoxResult> ShowInfoAsync(
            string title,
            string content,
            string closeButtonText
        )
        {
            Wpf.Ui.Controls.MessageBox messageBox = new()
            {
                Title = title,
                Content = content,
                IsPrimaryButtonEnabled = false,
                CloseButtonText = closeButtonText,
            };

            return await messageBox.ShowDialogAsync();
        }
    }
}
