using MyMoney.Core.FS.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyMoney.Views.Controls
{
    /// <summary>
    /// Interaction logic for BudgetReportControl.xaml
    /// </summary>
    public partial class BudgetReportControl : UserControl
    {
        public BudgetReportControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BudgetReportControl), new PropertyMetadata("Budget Report"));
        
        /// <summary>
        /// The title to display on the budget report
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty IncomeItemsProperty = 
            DependencyProperty.Register("IncomeItems", typeof(ObservableCollection<BudgetReportItem>), 
                typeof(BudgetReportControl), new PropertyMetadata(new ObservableCollection<BudgetReportItem>()));

        /// <summary>
        /// The report's income items
        /// </summary>
        public ObservableCollection<BudgetReportItem> IncomeItems
        {
            get { return (ObservableCollection<BudgetReportItem>)GetValue(IncomeItemsProperty); }
            set { SetValue(IncomeItemsProperty, value); }
        }

        public static readonly DependencyProperty ExpenseItemsProperty = DependencyProperty.Register("ExpenseItems",
            typeof(ObservableCollection<BudgetReportItem>), typeof(BudgetReportControl),
            new PropertyMetadata(new ObservableCollection<BudgetReportItem>()));

        /// <summary>
        /// The report's expense items
        /// </summary>
        public ObservableCollection<BudgetReportItem> ExpenseItems
        {
            get { return (ObservableCollection<BudgetReportItem>)GetValue(ExpenseItemsProperty); }
            set { SetValue(ExpenseItemsProperty, value); }
        }

        public static readonly DependencyProperty ReportTotalProperty = DependencyProperty.Register("ReportTotal",
            typeof(Currency), typeof(BudgetReportControl),
            new PropertyMetadata(new Currency(0m)));

        public Currency ReportTotal
        {
            get { return (Currency)GetValue(ReportTotalProperty); }
            set { SetValue(ReportTotalProperty, value); }
        }

        public static readonly DependencyProperty CategoryColumnWidthProperty = DependencyProperty.Register("CategoryColumnWidth",
            typeof(int), typeof(BudgetReportControl), new PropertyMetadata(200));

        /// <summary>
        /// The width of the category column
        /// </summary>
        public int CategoryColumnWidth
        {
            get { return (int)GetValue(CategoryColumnWidthProperty); }
            set { SetValue(CategoryColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty BudgetedColumnWidthProperty = DependencyProperty.Register("BudgetedColumnWidth",
            typeof(int), typeof(BudgetReportControl), new PropertyMetadata(100));

        /// <summary>
        /// The width of the budgeted column
        /// </summary>
        public int BudgetedColumnWidth
        {
            get { return (int)GetValue(BudgetedColumnWidthProperty); }
            set { SetValue(BudgetedColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty ActualColumnWidthProperty = DependencyProperty.Register("ActualColumnWidth",
            typeof(int), typeof(BudgetReportControl), new PropertyMetadata(100));

        /// <summary>
        /// The width of the actual column
        /// </summary>
        public int ActualColumnWidth
        {
            get { return (int)GetValue(ActualColumnWidthProperty); }
            set { SetValue(ActualColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty RemainingColumnWidthProperty = DependencyProperty.Register("RemainingColumnWidth",
            typeof(int), typeof(BudgetReportControl), new PropertyMetadata(100));

        /// <summary>
        /// The width of the remaining column
        /// </summary>
        public int RemainingColumnWidth
        {
            get { return (int)GetValue(RemainingColumnWidthProperty); }
            set { SetValue(RemainingColumnWidthProperty, value); }
        }
    }
}
