using MyMoney.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ViewModel.SaveBackupSettingsCommand.Execute(null);
        }
    }
}
