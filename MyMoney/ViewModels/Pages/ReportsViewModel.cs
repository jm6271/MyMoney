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
        public List<ScottPlot.Tick> IncomeExpense12Months_Labels = [];

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
            var ie12mc_lbl_str = IncomeExpense12MonthCalculator.GetMonthNames();

            IncomeExpense12Months_Labels.Clear();

            for (int i = 0; i < ie12mc_lbl_str.Count; i++) 
            {
                IncomeExpense12Months_Labels.Add(new(i, ie12mc_lbl_str[i]));
            }
        }
    }
}
