using MyMoney.ViewModels.ContentDialogs;
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

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for SavingsCategoryDialog.xaml
    /// </summary>
    public partial class SavingsCategoryDialog : Wpf.Ui.Controls.ContentDialog
    {
        public SavingsCategoryDialog(ContentPresenter dialogHost, SavingsCategoryDialogViewModel viewModel) : base(dialogHost)
        {
            InitializeComponent();
            DataContext = viewModel;
            TxtCategory.Focus();
        }
    }
}
