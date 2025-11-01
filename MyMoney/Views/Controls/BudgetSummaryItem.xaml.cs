using MyMoney.Core.Models;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Appearance;

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
            control.OnPropertyChanged(nameof(PercentageColorBrush));
            control.OnPropertyChanged(nameof(RemainingAmountTextColorBrush));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Currency ActualAmount
        {
            get { return (Currency)GetValue(ActualAmountProperty); }
            set { SetValue(ActualAmountProperty, value); }
        }

        public static readonly DependencyProperty RemainingAmountProperty =
            DependencyProperty.Register(nameof(RemainingAmount),
                typeof(Currency), typeof(BudgetSummaryItem), new PropertyMetadata(new Currency(0m), OnValueChanged));

        public Currency RemainingAmount
        {
            get => (Currency)GetValue(RemainingAmountProperty);
            set { SetValue(RemainingAmountProperty, value); }
        }

        public static readonly DependencyProperty IsExpenseProperty =
            DependencyProperty.Register(nameof(IsExpense),
                typeof(bool), typeof(BudgetSummaryItem), new PropertyMetadata(false, OnValueChanged));

        public bool IsExpense
        {
            get => (bool)GetValue(IsExpenseProperty);
            set { SetValue(IsExpenseProperty, value); }
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

        public Brush PercentageColorBrush
        {
            get
            {
                if (RemainingAmount.Value < 0 && IsExpense)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    // Accent color
                    return new SolidColorBrush(ApplicationAccentColorManager.PrimaryAccent);
                }
            }
        }

        public Brush RemainingAmountTextColorBrush
        {
            get
            {
                if (RemainingAmount.Value >= 0)
                {
                    return (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
                }
                else
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }
        }
    }
}
