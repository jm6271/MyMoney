using MyMoney.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace MyMoney.ViewAdapters.Pages
{
    public class DashboardViewAdapter
    {
        public ListCollectionView GroupedBudgetReportItemsView { get; }

        public DashboardViewAdapter(ObservableCollection<BudgetReportItem> reportItems)
        {
            GroupedBudgetReportItemsView = new(reportItems);
            GroupedBudgetReportItemsView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

        }
    }
}
