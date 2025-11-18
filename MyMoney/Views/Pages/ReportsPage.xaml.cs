using System.Windows.Controls;
using MyMoney.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : INavigableView<ReportsViewModel>
    {
        public ReportsViewModel ViewModel { get; }

        public ReportsPage(ReportsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
