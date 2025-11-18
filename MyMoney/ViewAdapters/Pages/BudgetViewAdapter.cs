using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MyMoney.Core.Models;
using MyMoney.ViewModels.Pages;

namespace MyMoney.ViewAdapters.Pages
{
    public class BudgetViewAdapter
    {
        public ListCollectionView GroupedBudgetsView { get; }

        public BudgetViewAdapter(ObservableCollection<GroupedBudget> budgets)
        {
            GroupedBudgetsView = new(budgets);
            GroupedBudgetsView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            GroupedBudgetsView.CustomSort = new GroupComparer();
        }

        private sealed class GroupComparer : System.Collections.IComparer
        {
            private readonly Dictionary<string, int> _groupOrder = new()
            {
                { "Current", 0 },
                { "Future", 1 },
                { "Past", 2 },
            };

            public int Compare(object? x, object? y)
            {
                if (x is not GroupedBudget group1 || y is not GroupedBudget group2)
                    return 0;

                string name1 = group1.Group;
                string name2 = group2.Group;

                // If the group names are in our dictionary, use the custom order
                if (_groupOrder.TryGetValue(name1, out int value) && _groupOrder.TryGetValue(name2, out int value2))
                {
                    return value.CompareTo(value2);
                }

                // Fall back to alphabetical sorting for any other groups
                return string.Compare(name1, name2);
            }
        }
    }
}
