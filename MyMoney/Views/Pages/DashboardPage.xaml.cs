using System.Windows.Controls;
using MyMoney.ViewAdapters.Pages;
using MyMoney.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardViewAdapter ViewAdapter { get; }

        // Cache the “wide” padding so we can restore it
        private Thickness _wideBudgetOverviewMargin;
        private Thickness _wideRightPanelMargin;

        // Define the “narrow” padding (no left inset)
        private readonly Thickness _narrowMargin = new Thickness(0, 0, 0, 24);

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewAdapter = new(viewModel.BudgetReportItems);
            DataContext = this;

            InitializeComponent();

            _wideBudgetOverviewMargin = budgetOverviewPanel.Margin;
            _wideRightPanelMargin = RightPanel.Margin;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 850)
            {
                // Switch to narrow layout
                Col1.Width = new GridLength(0);
                Grid.SetColumn(RightPanel, 0);
                Grid.SetRow(RightPanel, 2);
                RightPanel.Margin = _narrowMargin;
                budgetOverviewPanel.Margin = _narrowMargin;
            }
            else
            {
                // Switch to wide layout
                Col1.Width = new GridLength(3, GridUnitType.Star);
                Grid.SetColumn(RightPanel, 1);
                Grid.SetRow(RightPanel, 1);
                RightPanel.Margin = _wideRightPanelMargin;
                budgetOverviewPanel.Margin = _wideBudgetOverviewMargin;
            }
        }
    }
}
