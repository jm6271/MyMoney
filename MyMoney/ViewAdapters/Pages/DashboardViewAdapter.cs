using System.Collections.ObjectModel;
using System.Windows.Data;
using MyMoney.Core.Models;

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
