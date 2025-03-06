using System.Collections.ObjectModel;
using System.Globalization;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewBudgetDialogViewModel : ObservableObject
    {
        public ObservableCollection<string> AvailableBudgetDates { get; } = [];

        [ObservableProperty]
        private bool _useLastMonthsBudget = true;

        [ObservableProperty]
        private int _selectedDateIndex = -1;

        [ObservableProperty]
        private string _selectedDate = string.Empty;

        public NewBudgetDialogViewModel()
        {
            // Load the available month names
            var dt = DateTime.Now;
            AvailableBudgetDates.Add(dt.ToString("MMMM, yyyy", CultureInfo.InvariantCulture));
            AvailableBudgetDates.Add(dt.AddMonths(1).ToString("MMMM, yyyy", CultureInfo.InvariantCulture));
            SelectedDateIndex = 1;
            SelectedDate = AvailableBudgetDates[1];
        }
    }
}
