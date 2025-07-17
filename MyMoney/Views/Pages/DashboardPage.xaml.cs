using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        private Thickness _wideReportMargin;
        private Thickness _wideRightPanelMargin;

        private Thickness _narrowReportMargin = new(0, 0, 0, 8);
        private Thickness _narrowRightPanelMargin = new(0, 8, 0, 0);

        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            _wideReportMargin = BudgetReport.Margin;
            _wideRightPanelMargin = RightPanel.Margin;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 875)
            {
                // Switch to narrow layout
                Col0.Width = new GridLength(1, GridUnitType.Star);
                Col1.Width = new GridLength(0);
                Grid.SetColumn(RightPanel, 0);
                Grid.SetRow(RightPanel, 1);
                Grid.SetRowSpan(RightPanel, 1);
                BudgetReport.Margin = _narrowReportMargin;
                RightPanel.Margin = _narrowRightPanelMargin;
            }
            else
            {
                // Switch to wide layout
                Col0.Width = new GridLength(2, GridUnitType.Star);
                Col1.Width = new GridLength(1, GridUnitType.Star);
                Grid.SetColumn(RightPanel, 1);
                Grid.SetRow(RightPanel, 0);
                Grid.SetRowSpan(RightPanel, 2);
                BudgetReport.Margin = _wideReportMargin;
                RightPanel.Margin = _wideRightPanelMargin;
            }
        }
    }
}
