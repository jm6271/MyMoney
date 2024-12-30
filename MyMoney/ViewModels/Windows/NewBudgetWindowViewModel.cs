using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MyMoney.ViewModels.Windows
{
    public partial class NewBudgetWindowViewModel : ObservableObject
    {
        public ObservableCollection<string> AvailableBudgetDates { get; set; } = [];

        [ObservableProperty]
        private bool _UseLastMonthsBudget = true;

        [ObservableProperty]
        private int _SelectedDateIndex = -1;

        [ObservableProperty]
        private string _SelectedDate = string.Empty;

        public NewBudgetWindowViewModel()
        {
            // Load the available month names
            DateTime dt = DateTime.Now;
            AvailableBudgetDates.Add(dt.ToString("MMMM yyyy", CultureInfo.InvariantCulture));
            AvailableBudgetDates.Add(dt.AddMonths(1).ToString("MMMM yyyy", CultureInfo.InvariantCulture));
            SelectedDateIndex = 1;
        }
    }
}
