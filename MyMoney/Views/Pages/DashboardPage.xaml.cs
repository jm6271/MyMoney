using MyMoney.ViewAdapters.Pages;
using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardViewAdapter ViewAdapter { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewAdapter = new(viewModel.BudgetReportItems);
            DataContext = this;

            InitializeComponent();
        }
    }
}
