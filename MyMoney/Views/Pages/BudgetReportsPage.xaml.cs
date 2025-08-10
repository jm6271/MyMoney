using MyMoney.ViewModels.Pages.ReportPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for BudgetReportsPage.xaml
    /// </summary>
    public partial class BudgetReportsPage : INavigableView<BudgetReportsViewModel>
    {
        public BudgetReportsViewModel ViewModel { get; private set; }

        private readonly Thickness _narrowBudgetsMargin = new(0, 0, 0, 24);
        private readonly Thickness _narrowReportMargin = new(0, 0, 0, 8);
        private readonly Thickness _narrowIncomeChartMargin = new(0, 8, 8, 24);

        private Thickness _wideBudgetsMargin;
        private Thickness _wideReportMargin;
        private Thickness _wideIncomeChartMargin;

        public BudgetReportsPage(BudgetReportsViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = this;

            _wideBudgetsMargin = BudgetsCard.Margin;
            _wideReportMargin = BudgetReport.Margin;
            _wideIncomeChartMargin = IncomeChart.Margin;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 750)
            {
                // Switch to narrow layout
                Col0.Width = new GridLength(0);
                Row1.Height = new GridLength(0, GridUnitType.Auto);
                Row2.Height = new GridLength(0, GridUnitType.Auto);
                Grid.SetColumn(BudgetsCard, 1);
                Grid.SetRowSpan(BudgetsCard, 1);
                Grid.SetColumnSpan(BudgetsCard, 2);
                Grid.SetRow(BudgetsCard, 0);
                Grid.SetRow(BudgetReport, 1);
                Grid.SetRow(IncomeChart, 2);
                Grid.SetRow(ExpenseChart, 2);
                BudgetsListView.MaxHeight = 300;

                // Adjust margins
                BudgetsCard.Margin = _narrowBudgetsMargin;
                BudgetReport.Margin = _narrowReportMargin;
                IncomeChart.Margin = _narrowIncomeChartMargin;
            }
            else
            {
                // Switch to wide layout
                Col0.Width = new GridLength(200);
                Row1.Height = new GridLength(1, GridUnitType.Star);
                Row2.Height = new GridLength(0);
                Grid.SetColumn(BudgetsCard, 0);
                Grid.SetRow(BudgetsCard, 0);
                Grid.SetRowSpan(BudgetsCard, 2);
                Grid.SetColumnSpan(BudgetsCard, 1);
                Grid.SetRow(BudgetReport, 0);
                Grid.SetRow(IncomeChart, 1);
                Grid.SetRow(ExpenseChart, 1);
                BudgetsListView.MaxHeight = double.PositiveInfinity;

                // Adjust margins
                BudgetsCard.Margin = _wideBudgetsMargin;
                BudgetReport.Margin= _wideReportMargin;
                IncomeChart.Margin = _wideIncomeChartMargin;
            }
        }
    }
}
