using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using ScottPlot;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public ReportsViewModel ViewModel { get; }

        public ReportsPage(ReportsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            Initialize12MonthIncomeExpenseChart();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Update the chart values in the view model
            ViewModel.PageNavigatedToCommand.Execute(this);

            // Apply changes to the charts
            UpdateCharts();
        }

        private void UpdateCharts()
        {
            Update12MonthIncomeExpenseChart();
        }

        private void Update12MonthIncomeExpenseChart()
        {
            IncomeExpensePlot12Months.Plot.Clear();
            var bars = IncomeExpensePlot12Months.Plot.Add.Bars(ViewModel.IncomeExpense12Months_BarValues.ToArray());
            IncomeExpensePlot12Months.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual([.. ViewModel.IncomeExpense12Months_Labels]);
            IncomeExpensePlot12Months.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
            IncomeExpensePlot12Months.Plot.Axes.Margins(bottom: 0);

            // Set the bar color to the windows accent color
            var accentColor = ApplicationAccentColorManager.GetColorizationColor();
            bars.Color = new(accentColor.R, accentColor.G, accentColor.B, accentColor.A);

            // If we're in dark mode, change the colors of the chart
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark)
            {
                IncomeExpensePlot12Months.Plot.FigureBackground.Color = Color.FromHex("#323232");
                IncomeExpensePlot12Months.Plot.DataBackground.Color = Color.FromHex("#272727");
                IncomeExpensePlot12Months.Plot.Axes.Color(Color.FromHex("#d7d7d7"));
                IncomeExpensePlot12Months.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
                IncomeExpensePlot12Months.Plot.Axes.Left.TickLabelStyle.ForeColor = Color.FromHex("#cdcdcd");
                IncomeExpensePlot12Months.Plot.Axes.Bottom.TickLabelStyle.ForeColor = Color.FromHex("#cdcdcd");
            }
            else
            {
                // Change to light mode
                IncomeExpensePlot12Months.Plot.FigureBackground.Color = Color.FromHex("#fefefe");
                IncomeExpensePlot12Months.Plot.DataBackground.Color = Color.FromHex("#fafafa");
                IncomeExpensePlot12Months.Plot.Axes.Color(Color.FromHex("#333333"));
                IncomeExpensePlot12Months.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
                IncomeExpensePlot12Months.Plot.Axes.Left.TickLabelStyle.ForeColor = Color.FromHex("#333333");
                IncomeExpensePlot12Months.Plot.Axes.Bottom.TickLabelStyle.ForeColor = Color.FromHex("#333333");
            }

            IncomeExpensePlot12Months.Refresh();
        }

        private void Initialize12MonthIncomeExpenseChart()
        {
            IncomeExpensePlot12Months.Plot.Clear();
            var bars = IncomeExpensePlot12Months.Plot.Add.Bars(ViewModel.IncomeExpense12Months_BarValues.ToArray());
            IncomeExpensePlot12Months.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual([.. ViewModel.IncomeExpense12Months_Labels]);
            IncomeExpensePlot12Months.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
            IncomeExpensePlot12Months.Plot.YLabel("Dollar Amount");
            IncomeExpensePlot12Months.Plot.Title("Income vs Expenses Past 12 Months");
            IncomeExpensePlot12Months.Plot.Axes.Margins(bottom: 0);
            IncomeExpensePlot12Months.Plot.Axes.Title.Label.FontSize = 48;
            IncomeExpensePlot12Months.Plot.Axes.Left.Label.FontSize = 32;
            IncomeExpensePlot12Months.Plot.Axes.Left.TickLabelStyle.FontSize = 24;
            IncomeExpensePlot12Months.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;

            // Set the bar color to the windows accent color
            var accentColor = ApplicationAccentColorManager.GetColorizationColor();
            bars.Color = new(accentColor.R, accentColor.G, accentColor.B, accentColor.A);

            IncomeExpensePlot12Months.Refresh();
        }
    }
}
