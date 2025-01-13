using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyMoney.App.ViewModels;

namespace MyMoney.App.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(MainViewModel));
        }

        private void MainNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string? tag = args.SelectedItemContainer.Tag.ToString();
                if (tag != null)
                {
                    switch (tag)
                    {
                        case "HomePage":
                            ContentFrame.Navigate(typeof(HomePage));
                            break;
                        case "AccountsPage":
                            ContentFrame.Navigate(typeof(AccountsPage));
                            break;
                        case "BudgetPage":
                            ContentFrame.Navigate(typeof(BudgetPage));
                            break;
                        case "ReportsPage":
                            ContentFrame.Navigate(typeof(ReportsPage));
                            break;
                    }
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(HomePage));
            MainNavigation.SelectedItem = MainNavigation.MenuItems[0];
        }
    }
}
