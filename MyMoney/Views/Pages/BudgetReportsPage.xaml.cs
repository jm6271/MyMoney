using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using MyMoney.Helpers;
using MyMoney.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for BudgetReportsPage.xaml
    /// </summary>
    public partial class BudgetReportsPage : INavigableView<BudgetReportsViewModel>
    {
        public BudgetReportsViewModel ViewModel { get; private set; }

        public BudgetReportsPage(BudgetReportsViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = this;

            // Listen for MouseWheel on *all* column headers inside this page
            AddHandler(
                GridViewColumnHeader.PreviewMouseWheelEvent,
                new MouseWheelEventHandler(GridViewColumnHeader_PreviewMouseWheel),
                handledEventsToo: true
            );
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

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            scrollViewer.ScrollToVerticalOffset(
                scrollViewer.VerticalOffset - e.Delta / 3.0);

            e.Handled = true;
        }
    }
}
