using Microsoft.UI.Xaml.Controls;

namespace MyMoney.App.Views
{
    public sealed partial class BudgetPage : Page
    {
        public BudgetPage()
        {
            this.InitializeComponent();
            this.DataContext = App.ServiceProvider.GetService(typeof(BudgetPage));
        }
    }
}
