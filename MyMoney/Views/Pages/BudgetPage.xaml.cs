using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for BudgetPage.xaml
    /// </summary>
    public partial class BudgetPage : INavigableView<BudgetViewModel>
    {
        private Thickness _budgetsListWideMargin;
        private Thickness _budgetsPanelWideMargin;
        private Thickness _chartPanelWideMargin;
        private Thickness _incomeChartWideMargin;
        private Thickness _expenseChartWideMargin;

        private Thickness _budgetsListNarrowMargin = new(0, 0, 0, 12);
        private Thickness _budgetsPanelNarrowMargin = new(0, 12, 0, 4);
        private Thickness _chartPanelNarrowMargin = new(0, 12, 0, 24);
        private Thickness _incomeChartNarrowMargin = new(0, 0, 8, 0);
        private Thickness _expenseChartNarrowMargin = new(8, 0, 0, 0);

        public BudgetViewModel ViewModel { get; }

        public BudgetPage(BudgetViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            _budgetsListWideMargin = BudgetsCard.Margin;
            _budgetsPanelWideMargin = BudgetPanel.Margin;
            _chartPanelWideMargin = ChartsPanel.Margin;
            _incomeChartWideMargin = IncomeChart.Margin;
            _expenseChartWideMargin= ExpenseChart.Margin;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Change the width of the first column of the expense listview whose size has changed
            if (sender is Wpf.Ui.Controls.ListView listView)
            {
                int w = (int)(listView.ActualWidth - 280); // width of other columns plus some extra for padding
                if (w < 100) w = 100;
                ((Wpf.Ui.Controls.GridView)listView.View).Columns[0].Width = w;
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 900)
            {
                // Switch to narrow layout
                Col0.Width = new GridLength(0);
                Col2.Width = new GridLength(0);
                Row0.Height = new GridLength(1, GridUnitType.Auto);
                Row1.Height = new GridLength(1, GridUnitType.Star);
                Row2.Height = new GridLength(1, GridUnitType.Auto);
                Grid.SetColumn(BudgetsCard, 1);
                Grid.SetColumn(ChartsPanel, 1);
                Grid.SetRow(BudgetPanel, 1);
                Grid.SetRow(ChartsPanel, 2);
                BudgetsListView.MaxHeight = 300;

                // Rearrange the charts
                ChartRow1.Height = new GridLength(0);
                ChartCol1.Width = new GridLength(1, GridUnitType.Star);
                Grid.SetColumn(ExpenseChart, 1);
                Grid.SetRow(ExpenseChart, 0);

                // Set margins
                BudgetsCard.Margin = _budgetsListNarrowMargin;
                BudgetPanel.Margin = _budgetsPanelNarrowMargin;
                ChartsPanel.Margin = _chartPanelNarrowMargin;
                IncomeChart.Margin = _incomeChartNarrowMargin;
                ExpenseChart.Margin = _expenseChartNarrowMargin;
            }
            else
            {
                // Switch to wide layout
                Col0.Width = new GridLength(1, GridUnitType.Star);
                Col2.Width = new GridLength(1, GridUnitType.Star);
                Row0.Height = new GridLength(1, GridUnitType.Star);
                Row1.Height = new GridLength(0);
                Row2.Height = new GridLength(0);
                Grid.SetColumn(BudgetsCard, 0);
                Grid.SetColumn(ChartsPanel, 2);
                Grid.SetRow(BudgetPanel, 0);
                Grid.SetRow(ChartsPanel, 0);
                BudgetsListView.MaxHeight = double.PositiveInfinity;

                // Rearrange the charts
                ChartRow1.Height = new GridLength(1, GridUnitType.Auto);
                ChartCol1.Width = new GridLength(0);
                Grid.SetColumn(ExpenseChart, 0);
                Grid.SetRow(ExpenseChart, 1);

                // Set margins
                BudgetsCard.Margin = _budgetsListWideMargin;
                BudgetPanel.Margin = _budgetsPanelWideMargin;
                ChartsPanel.Margin = _chartPanelWideMargin;
                IncomeChart.Margin = _incomeChartWideMargin;
                ExpenseChart.Margin = _expenseChartWideMargin;
            }
        }

        private void CardExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ViewModel.WriteToDatabase();
        }

        private void CardExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ViewModel.WriteToDatabase();
        }
    }
}
