using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for GroupedComboBox.xaml
    /// </summary>
    public partial class GroupedComboBox : UserControl, INotifyPropertyChanged
    {
        public ListCollectionView GroupedItems { get; set; }

        public GroupedComboBox()
        {
            InitializeComponent();
            GroupedItems = new(ItemsSource);
        }

        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(SelectedIndexProperty);
            }
            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex",
            typeof(int), typeof(GroupedComboBox), new PropertyMetadata(-1));


        private GroupedComboBoxItem? _selectedItem = null;
        public GroupedComboBoxItem? SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    dropDownPopup.IsOpen = false;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public string Text => SelectedItem?.Item.ToString() ?? string.Empty;

        public ObservableCollection<GroupedComboBoxItem> ItemsSource
        {
            get
            {
                return (ObservableCollection<GroupedComboBoxItem>)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
                GroupedItems = new(ItemsSource);
                GroupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(ObservableCollection<GroupedComboBoxItem>), typeof(GroupedComboBox), 
            new PropertyMetadata(new ObservableCollection<GroupedComboBoxItem>()));

        

        public class GroupedComboBoxItem
        {
            public string Group { get; set; } = "";
            public object Item { get; set; } = "";
        }

        private void dropdownButton_Click(object sender, RoutedEventArgs e)
        {
            // if dropdown is not down, then close it
            if (!dropDownPopup.IsOpen)
            {
                dropDownPopup.IsOpen = true;
            }
            else
            {
                dropDownPopup.IsOpen = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
