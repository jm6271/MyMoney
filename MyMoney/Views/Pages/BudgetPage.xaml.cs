using MyMoney.ViewModels.Pages;
using System.Windows.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for BudgetPage.xaml
    /// </summary>
    public partial class BudgetPage : Page
    {
        public BudgetViewModel ViewModel { get; }

        public BudgetPage(BudgetViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();
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
    }
}
