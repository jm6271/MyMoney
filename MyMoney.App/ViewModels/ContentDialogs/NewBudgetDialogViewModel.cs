using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.App.ViewModels.ContentDialogs
{
    public partial class NewBudgetDialogViewModel : ObservableObject
    {
        public ObservableCollection<string> AvailableBudgetDates { get; set; } = [];

        [ObservableProperty]
        public partial bool UseLastMonthsBudget { get; set; } = true;

        [ObservableProperty]
        public partial int SelectedDateIndex { get; set; }

        [ObservableProperty]
        public partial string SelectedDate { get; set; }

        public NewBudgetDialogViewModel()
        {
            // Load the available month names
            DateTime dt = DateTime.Now;
            AvailableBudgetDates.Add(dt.ToString("MMMM, yyyy", CultureInfo.InvariantCulture));
            AvailableBudgetDates.Add(dt.AddMonths(1).ToString("MMMM, yyyy", CultureInfo.InvariantCulture));
            SelectedDateIndex = 1;
            SelectedDate = AvailableBudgetDates[1];
        }
    }
}
