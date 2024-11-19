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
    /// Interaction logic for TransferWindow.xaml
    /// </summary>
    public partial class TransferWindow : FluentWindow
    {
        public TransferWindowViewModel ViewModel { get; set; }

        public TransferWindow(TransferWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void BttnTransfer_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void txtAmount_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (txtAmount.Value == null) ViewModel.Amount = new(0);
            else
            {
                ViewModel.Amount = new((decimal)txtAmount.Value);
            }
        }
    }
}
