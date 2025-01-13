using Microsoft.UI.Xaml.Controls;


namespace MyMoney.App.Views
{
    public sealed partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            this.InitializeComponent();
            this.DataContext = App.ServiceProvider.GetService(typeof(ReportsPage));
        }
    }
}
