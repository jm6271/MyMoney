using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Services.ContentDialogs
{
    public interface IMessageBoxService
    {
        public Task<Wpf.Ui.Controls.MessageBoxResult> ShowAsync(Wpf.Ui.Controls.MessageBox messageBox);
    }

    public class MessageBoxService : IMessageBoxService
    {
        public async Task<Wpf.Ui.Controls.MessageBoxResult> ShowAsync(Wpf.Ui.Controls.MessageBox messageBox)
        {
            return await messageBox.ShowDialogAsync();
        }
    }
}
