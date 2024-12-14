using MyMoney.ViewModels.Pages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();
        }
    }
}
