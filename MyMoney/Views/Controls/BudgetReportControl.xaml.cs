using MyMoney.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyMoney.Views.Controls
{
    /// <summary>
    /// Interaction logic for BudgetReportControl.xaml
    /// </summary>
    public partial class BudgetReportControl : UserControl, INotifyPropertyChanged
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

        public static readonly DependencyProperty SavingsItemsProperty =
            DependencyProperty.Register("SavingsItems", typeof(ObservableCollection<SavingsCategoryReportItem>),
                typeof(BudgetReportControl), new PropertyMetadata(new ObservableCollection<SavingsCategoryReportItem>()));

        /// <summary>
        /// The report's savings items
        /// </summary>
        public ObservableCollection<SavingsCategoryReportItem> SavingsItems
        {
            get { return (ObservableCollection<SavingsCategoryReportItem>)GetValue(SavingsItemsProperty); }
            set { SetValue(SavingsItemsProperty, value); }
        }

        public static readonly DependencyProperty ExpenseItemsProperty = DependencyProperty.Register("ExpenseItems",
            typeof(ObservableCollection<BudgetReportItem>), typeof(BudgetReportControl),
            new FrameworkPropertyMetadata(new ObservableCollection<BudgetReportItem>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnExpenseItemsChanged));

        /// <summary>
        /// The report's expense items
        /// </summary>
        public ObservableCollection<BudgetReportItem> ExpenseItems
        {
            get { return (ObservableCollection<BudgetReportItem>)GetValue(ExpenseItemsProperty); }
            set { SetValue(ExpenseItemsProperty, value); }
        }


        // This gets called when the entire collection is replaced
        private static void OnExpenseItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BudgetReportControl)d;

            // Unsubscribe from old collection events (if any)
            if (e.OldValue is ObservableCollection<BudgetReportItem> oldCollection)
            {
                oldCollection.CollectionChanged -= control.UpdateGroupedExpenseItems;
            }

            // Subscribe to new collection events
            if (e.NewValue is ObservableCollection<BudgetReportItem> newCollection)
            {
                newCollection.CollectionChanged += control.UpdateGroupedExpenseItems;
            }
        }


        // This gets called when items are added/removed/replaced
        private void UpdateGroupedExpenseItems(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(GroupedExpenseItems));
        }


        // grouped items to display in the ListView
        public ListCollectionView GroupedExpenseItems
        {
            get
            {
                ListCollectionView listCollectionView = new(ExpenseItems);
                listCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                return listCollectionView;
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
