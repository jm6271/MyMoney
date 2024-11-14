using MyMoney.ViewModels.Windows;

namespace MyMoney.Views.Windows
{
    /// <summary>
    /// Interaction logic for NewAccountWindow.xaml
    /// </summary>
    public partial class NewAccountWindow : Wpf.Ui.Controls.FluentWindow
    {
        public NewAccountWindowViewModel ViewModel { get; set; }

        public NewAccountWindow(NewAccountWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtAccountName.Focus();
        }

        private void TxtAccountName_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtAccountName.SelectAll();
        }

        private void txtStartingBalance_GotFocus(object sender, RoutedEventArgs e)
        {
            txtStartingBalance.SelectAll();
        }
    }
}
