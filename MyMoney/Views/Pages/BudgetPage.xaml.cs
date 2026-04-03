using System.Windows.Controls;
using System.Windows.Input;
using MyMoney.Core.Models;
using MyMoney.Helpers;
using MyMoney.ViewAdapters.Pages;
using MyMoney.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for BudgetPage.xaml
    /// </summary>
    public partial class BudgetPage : INavigableView<BudgetViewModel>
    {
        public BudgetViewModel ViewModel { get; }

        public BudgetPage(BudgetViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            // Listen for MouseWheel on *all* column headers inside this page
            AddHandler(
                GridViewColumnHeader.PreviewMouseWheelEvent,
                new MouseWheelEventHandler(GridViewColumnHeader_PreviewMouseWheel),
                handledEventsToo: true
            );
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Change the width of the first column of the expense listview whose size has changed
            if (sender is Wpf.Ui.Controls.ListView listView)
            {
                int w = (int)(listView.ActualWidth - 280); // width of other columns plus some extra for padding
                if (w < 100)
                    w = 100;
                ((Wpf.Ui.Controls.GridView)listView.View).Columns[0].Width = w;
            }
        }

        private void CardExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ViewModel.WriteToDatabase();
        }

        private void GridViewColumnHeader_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Don't let the header eat the scroll event
            e.Handled = false;

            var scrollViewer = (sender as DependencyObject)?.FindAncestor<ScrollViewer>();
            if (scrollViewer != null)
            {
                scrollViewer.RaiseEvent(
                    new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender,
                    }
                );
            }
        }

        private void MainScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            scrollViewer.ScrollToVerticalOffset(
                scrollViewer.VerticalOffset - e.Delta / 3.0);

            e.Handled = true;
        }
    }
}
