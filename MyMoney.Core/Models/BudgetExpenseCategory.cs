using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using LiteDB;

namespace MyMoney.Core.Models
{
    public partial class BudgetExpenseCategory : ObservableObject, ICloneable
    {
        [ObservableProperty]
        private int _id = 0;

        [ObservableProperty]
        private string _categoryName = "";

        [ObservableProperty]
        private ObservableCollection<BudgetItem> _subItems = [];

        [ObservableProperty]
        [property: BsonIgnore]
        private int _selectedSubItemIndex = -1;

        [ObservableProperty]
        private bool _isExpanded = true;

        public Currency CategoryTotal
        {
            get
            {
                decimal total = 0;
                foreach (var item in SubItems)
                {
                    total += Math.Abs(item.Amount.Value);
                }
                return new Currency(total);
            }
        }

        public object Clone()
        {
            return new BudgetExpenseCategory()
            {
                Id = this.Id,
                CategoryName = this.CategoryName,
                SubItems = new ObservableCollection<BudgetItem>(this.SubItems.Select(item => (BudgetItem)item.Clone())),
                IsExpanded = this.IsExpanded
            };
        }
    }
}
