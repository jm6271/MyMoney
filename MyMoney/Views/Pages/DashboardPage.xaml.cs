using MyMoney.ViewModels.Pages;
using ScottPlot;
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

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeChart();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.BarValues))
            {
                UpdateChart();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();
        }

        private void InitializeChart()
        {
            IncomeExpenseChart.Plot.Clear();
            IncomeExpenseChart.Plot.Add.Bars(ViewModel.BarValues);
            IncomeExpenseChart.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ViewModel.BarLabels);
            IncomeExpenseChart.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
            IncomeExpenseChart.Plot.YLabel("Dollar Amount");
            IncomeExpenseChart.Plot.Title("Income vs Expenses");
            IncomeExpenseChart.Plot.Axes.Margins(bottom: 0);
            IncomeExpenseChart.Plot.Axes.Title.Label.FontSize = 48;
            IncomeExpenseChart.Plot.Axes.Left.Label.FontSize = 32;
            IncomeExpenseChart.Plot.Axes.Left.TickLabelStyle.FontSize = 24;
            IncomeExpenseChart.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;
            IncomeExpenseChart.Refresh();
        }

        private void UpdateChart()
        {
            IncomeExpenseChart.Plot.Clear();
            IncomeExpenseChart.Plot.Add.Bars(ViewModel.BarValues);
            IncomeExpenseChart.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ViewModel.BarLabels);
            IncomeExpenseChart.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
            IncomeExpenseChart.Plot.Axes.Margins(bottom: 0);

            // If we're in dark mode, change the colors of the chart
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark)
            {
                IncomeExpenseChart.Plot.FigureBackground.Color = Color.FromHex("#323232");
                IncomeExpenseChart.Plot.DataBackground.Color = Color.FromHex("#272727");
                IncomeExpenseChart.Plot.Axes.Color(Color.FromHex("#d7d7d7"));
                IncomeExpenseChart.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
                IncomeExpenseChart.Plot.Axes.Left.TickLabelStyle.ForeColor = Color.FromHex("#cdcdcd");
                IncomeExpenseChart.Plot.Axes.Bottom.TickLabelStyle.ForeColor = Color.FromHex("#cdcdcd");
            }

            IncomeExpenseChart.Refresh();
        }
    }
}
