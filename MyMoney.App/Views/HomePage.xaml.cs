using Microsoft.UI.Xaml.Controls;
using MyMoney.App.ViewModels;

namespace MyMoney.App.Views
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(HomeViewModel));
        }
    }
}
