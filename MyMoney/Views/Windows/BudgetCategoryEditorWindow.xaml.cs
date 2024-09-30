using MyMoney.ViewModels.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Windows
{
    /// <summary>
    /// Interaction logic for BudgetCategoryEditorWindow.xaml
    /// </summary>
    public partial class BudgetCategoryEditorWindow : FluentWindow
    {
        public BudgetCategoryEditorWindowViewModel ViewModel { get; set; }

        public BudgetCategoryEditorWindow(BudgetCategoryEditorWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelButtonClick();
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OkButtonClick();
            Close();
        }
    }
}
