using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyMoney.App.ViewModels;
using System;
using System.Threading.Tasks;

namespace MyMoney.App.Views
{
    public sealed partial class BudgetPage : Page
    {
        private readonly BudgetViewModel ViewModel;

        public BudgetPage()
        {
            this.InitializeComponent();
            ViewModel = new BudgetViewModel();
            DataContext = ViewModel;
        }

        private async void NewBudget_Clicked(object sender, RoutedEventArgs e)
        {
            await ViewModel.CreateNewBudget(XamlRoot);
        }
    }
}
