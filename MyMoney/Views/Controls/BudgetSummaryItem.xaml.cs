using MyMoney.Core.Models;
using System.ComponentModel;
using System.Windows.Controls;

namespace MyMoney.Views.Controls
{
    /// <summary>
    /// Interaction logic for BudgetSummaryItem.xaml
    /// </summary>
    public partial class BudgetSummaryItem : UserControl, INotifyPropertyChanged
    {
        public BudgetSummaryItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register(nameof(Category), 
                typeof(string), typeof(BudgetSummaryItem), new PropertyMetadata(string.Empty));

        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        public static readonly DependencyProperty BudgetedAmountProperty =
            DependencyProperty.Register(nameof(BudgetedAmount), 
                typeof(Currency), typeof(BudgetSummaryItem), new PropertyMetadata(new Currency(0m), OnValueChanged));

        public Currency BudgetedAmount
        {
            get { return (Currency)GetValue(BudgetedAmountProperty); }
            set { SetValue(BudgetedAmountProperty, value); }
        }

        public static readonly DependencyProperty ActualAmountProperty =
            DependencyProperty.Register(nameof(ActualAmount), 
                typeof(Currency), typeof(BudgetSummaryItem), new PropertyMetadata(new Currency(0m), OnValueChanged));


        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BudgetSummaryItem)d;

            control.OnPropertyChanged(nameof(ActualAmountPercentage));
            control.OnPropertyChanged(nameof(RemainingAmount));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Currency ActualAmount
        {
            get { return (Currency)GetValue(ActualAmountProperty); }
            set { SetValue(ActualAmountProperty, value); }
        }

        public Currency RemainingAmount
        {
            get
            {
                return BudgetedAmount - ActualAmount;
            }
        }

        public double ActualAmountPercentage
        {
            get
            {
                if (BudgetedAmount.Value == 0)
                    return 0;
                return Math.Min((double)(ActualAmount.Value / BudgetedAmount.Value) * 100, 100);
            }
        }
    }
}
