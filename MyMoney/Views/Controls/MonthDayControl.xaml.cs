using System;
using System.Collections.Generic;
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
    /// Interaction logic for MonthDayControl.xaml
    /// </summary>
    public partial class MonthDayControl : UserControl
    {
        public MonthDayControl()
        {
            InitializeComponent();
        }

        public string Month
        {
            get
            {
                return (string)GetValue(MonthProperty);
            }

            set
            {
                SetValue(MonthProperty, value);
            }
        }
        public static readonly DependencyProperty MonthProperty = DependencyProperty.Register("Month", typeof(string), typeof(MonthDayControl), new PropertyMetadata(string.Empty));
        
        
        public int Day
        {
            get
            {
                return (int)GetValue(DayProperty);
            }
            set
            {
                SetValue(DayProperty, value);
            }
        }
        public static readonly DependencyProperty DayProperty = DependencyProperty.Register("Day", typeof(int), typeof(MonthDayControl), new PropertyMetadata(0));
    }
}
