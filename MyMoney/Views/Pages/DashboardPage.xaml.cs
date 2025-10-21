using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
