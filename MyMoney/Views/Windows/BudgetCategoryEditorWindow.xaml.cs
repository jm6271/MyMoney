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
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OkButtonClick();
            DialogResult = true;
            Close();
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCategory.Focus();
        }

        private void TxtCategory_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtCategory.SelectAll();
        }

        private void txtAmount_GotFocus(object sender, RoutedEventArgs e)
        {
            txtAmount.SelectAll();
        }
    }
}
