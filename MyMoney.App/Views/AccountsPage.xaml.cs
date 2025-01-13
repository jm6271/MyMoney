using Microsoft.UI.Xaml.Controls;
using MyMoney.App.ViewModels;

namespace MyMoney.App.Views
{
    public sealed partial class AccountsPage : Page
    {
        public AccountsPage()
        {
            this.InitializeComponent();
            this.DataContext = App.ServiceProvider.GetService(typeof(AccountsViewModel));
        }
    }
}
