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
    }
}
