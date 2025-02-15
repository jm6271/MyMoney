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

namespace MyMoney.Views.Pages.ReportPages
{
    /// <summary>
    /// Interaction logic for BudgetReportsPage.xaml
    /// </summary>
    public partial class BudgetReportsPage : Page
    {
        public BudgetReportsViewModel ViewModel { get; private set; }
        public BudgetReportsPage(BudgetReportsViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = this;
        }
    }
}
