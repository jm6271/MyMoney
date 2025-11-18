using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(GroupedComboBox)
        );

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        protected virtual void OnSelectionChanged()
        {
            RoutedEventArgs args = new(SelectionChangedEvent);
            RaiseEvent(args);
        }

        public ListCollectionView GroupedItems { get; set; }

        public GroupedComboBox()
        {
            InitializeComponent();
            GroupedItems = new(ItemsSource);
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(GroupedComboBox),
            new PropertyMetadata(-1)
        );

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(GroupedComboBoxItem),
            typeof(GroupedComboBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged
            )
        );

        public GroupedComboBoxItem? SelectedItem
        {
            get => (GroupedComboBoxItem?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (GroupedComboBox)d;

            // Execute your custom logic when SelectedItem changes
            control.OnSelectedItemChanged();
        }

        private void OnSelectedItemChanged()
        {
            // Custom logic here
            HidePopUp();
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(SelectedItem));
        }

        public string Text
        {
            get
            {
                if (SelectedIndex == -1)
                {
                    return "Select an item...";
                }
                else
                {
                    return SelectedItem?.Item.ToString() ?? "";
                }
            }
        }

        public ObservableCollection<GroupedComboBoxItem> ItemsSource
        {
            get { return (ObservableCollection<GroupedComboBoxItem>)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
                GroupedItems = new(ItemsSource);
                GroupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(ObservableCollection<GroupedComboBoxItem>),
            typeof(GroupedComboBox),
            new PropertyMetadata(new ObservableCollection<GroupedComboBoxItem>(), OnItemsSourceChanged)
        );

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (GroupedComboBox)d;
            if (e.NewValue is ObservableCollection<GroupedComboBoxItem> newItemsSource)
            {
                control.GroupedItems = new ListCollectionView(newItemsSource);
                control.GroupedItems.GroupDescriptions.Clear();
                control.GroupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            }
        }

        public class GroupedComboBoxItem : IEquatable<GroupedComboBoxItem>
        {
            public string Group { get; set; } = "";
            public object Item { get; set; } = "";

            public static bool operator ==(GroupedComboBoxItem x, GroupedComboBoxItem y)
            {
                return EqualityComparer<GroupedComboBoxItem>.Default.Equals(x, y);
            }

            public static bool operator !=(GroupedComboBoxItem x, GroupedComboBoxItem y)
            {
                return !(x == y);
            }

            // IEquatable<T> implementation
            public bool Equals(GroupedComboBoxItem? other)
            {
                if (other is null)
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                // Compare Group and Item (using default equality for Item)
                return string.Equals(Group, other.Group, StringComparison.Ordinal)
                    && EqualityComparer<object?>.Default.Equals(Item, other.Item);
            }

            // Override of object.Equals
            public override bool Equals(object? obj)
            {
                if (obj is null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((GroupedComboBoxItem)obj);
            }

            // Override of object.GetHashCode
            public override int GetHashCode()
            {
                return HashCode.Combine(Group, Item);
            }
        }

        private void dropdownButton_Click(object sender, RoutedEventArgs e)
        {
            // if dropdown is not down, then close it
            if (!dropDownPopup.IsOpen)
            {
                ShowPopUp();
            }
            else
            {
                HidePopUp();
            }
        }

        private void ShowPopUp()
        {
            dropDownPopup.IsOpen = true;
            var openAnimation = (Storyboard)FindResource("OpenAnimation");
            openAnimation.Begin((FrameworkElement)dropDownPopup.Child);
            itemsList.ScrollIntoView(itemsList.SelectedItem);
        }

        private void HidePopUp()
        {
            var closeAnimation = (Storyboard)FindResource("CloseAnimation");
            closeAnimation.Completed += (s, e) => dropDownPopup.IsOpen = false;
            closeAnimation.Begin((FrameworkElement)dropDownPopup.Child);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSelectionChanged();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            // if this is the up or down key, scroll to the next item in the list
            if (e.Key == Key.Up && SelectedIndex > 0)
            {
                SelectedIndex--;
                e.Handled = true;
            }
            else if (e.Key == Key.Down && SelectedIndex < ItemsSource.Count - 1)
            {
                SelectedIndex++;
                e.Handled = true;
            }
        }

        private void dropDownPopup_Closed(object sender, EventArgs e)
        {
            // Delay focus just enough to allow animation to complete
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    dropdownButton.Focus();
                }),
                System.Windows.Threading.DispatcherPriority.Input
            );
        }
    }
}
