using MyMoney.ViewModels.Windows;
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
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Windows
{
    /// <summary>
    /// Interaction logic for NewBudgetDialog.xaml
    /// </summary>
    public partial class NewBudgetDialog : FluentWindow
    {
        public NewBudgetWindowViewModel ViewModel { get; set; }

        public NewBudgetDialog(NewBudgetWindowViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            DataContext = this;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
