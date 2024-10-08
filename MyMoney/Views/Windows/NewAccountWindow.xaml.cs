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
    }
}
