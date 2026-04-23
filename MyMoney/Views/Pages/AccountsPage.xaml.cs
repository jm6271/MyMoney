using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MyMoney.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class AccountsPage : INavigableView<AccountsViewModel>
    {
        bool _isLoaded = false;

        public AccountsViewModel ViewModel { get; }

        public AccountsPage(AccountsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();
            _isLoaded = true;
        }

        private async void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                ResetScroll(TransactionsList);
            }

            await ViewModel.SelectedAccountChanged();
        }

        private void ResetScroll(System.Windows.Controls.ListView list)
        {
            var scroll = FindScrollViewer(list);
            scroll?.ScrollToTop();
        }

        private ScrollViewer? FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer sv) return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                var result = FindScrollViewer(child);
                if (result != null) return result;
            }

            return null;
        }

        private async void TransactionsList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is not ScrollViewer viewer) return;

            // when within 200px of bottom
            if (viewer.VerticalOffset + viewer.ViewportHeight >= viewer.ExtentHeight - 200)
            {
                await ViewModel.LoadTransactions();
            }
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            AccountsList.SelectedItem = button.DataContext;

            // Walk up to the ListViewItem to grab its ContextMenu
            var listViewItem = ItemsControl.ContainerFromElement(AccountsList, button) as Wpf.Ui.Controls.ListViewItem;
            if (listViewItem?.ContextMenu is ContextMenu menu)
            {
                menu.PlacementTarget = button; // anchor it to the button visually
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menu.DataContext = button.DataContext; // ensure bindings in menu items work
                menu.IsOpen = true;
            }
        }
    }
}
