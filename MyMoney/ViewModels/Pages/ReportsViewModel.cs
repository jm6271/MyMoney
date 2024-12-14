using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMoney.Core.Reports;

namespace MyMoney.ViewModels.Pages
{
    public partial class ReportsViewModel : ObservableObject
    {
        public List<double> IncomeExpense12Months_BarValues = [];
        public List<string> IncomeExpense12Months_Labels = [];
        public List<double> IncomeExpense12Months_LabelPositions = [];

        public ReportsViewModel()
        {
            UpdateCharts();
        }

        [RelayCommand]
        private void OnPageNavigatedTo()
        {
            UpdateCharts();
        }

        private void UpdateCharts()
        {
            IncomeExpense12Months_BarValues = IncomeExpense12MonthCalculator.GetIncomeAndExpenses();
            IncomeExpense12Months_Labels = IncomeExpense12MonthCalculator.GetMonthNames();
            IncomeExpense12Months_LabelPositions = IncomeExpense12MonthCalculator.GetLabelPosistions();
        }
    }
}
