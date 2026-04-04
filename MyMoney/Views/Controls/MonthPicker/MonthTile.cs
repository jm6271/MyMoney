using System.Windows.Controls;
using System.Windows.Media;

namespace MyMoney.Views.Controls;

public class MonthTile : Control
{
    static MonthTile()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MonthTile),
            new FrameworkPropertyMetadata(typeof(MonthTile)));
    }

    // Month
    public static readonly DependencyProperty MonthProperty =
        DependencyProperty.Register(
            nameof(Month),
            typeof(DateTime),
            typeof(MonthTile),
            new PropertyMetadata(DateTime.MinValue, OnMonthChanged));

    private static void OnMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tile = (MonthTile)d;
        var value = (DateTime)e.NewValue;
        if (value != DateTime.MinValue && value.Day != 1)
            tile.Month = new DateTime(value.Year, value.Month, 1);
    }

    public DateTime Month
    {
        get => (DateTime)GetValue(MonthProperty);
        set => SetValue(MonthProperty, value);
    }

    // IsSelected
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(MonthTile),
            new PropertyMetadata(false));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    // IsCurrentMonth
    public static readonly DependencyProperty IsCurrentMonthProperty =
        DependencyProperty.Register(
            nameof(IsCurrentMonth),
            typeof(bool),
            typeof(MonthTile),
            new PropertyMetadata(false));

    public bool IsCurrentMonth
    {
        get => (bool)GetValue(IsCurrentMonthProperty);
        set => SetValue(IsCurrentMonthProperty, value);
    }

    // AccentColor
    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(
            nameof(AccentColor),
            typeof(Brush),
            typeof(MonthTile),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4))));

    public Brush AccentColor
    {
        get => (Brush)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    // TileWidth
    public static readonly DependencyProperty TileWidthProperty =
        DependencyProperty.Register(
            nameof(TileWidth),
            typeof(double),
            typeof(MonthTile),
            new PropertyMetadata(80.0));

    public double TileWidth
    {
        get => (double)GetValue(TileWidthProperty);
        set => SetValue(TileWidthProperty, value);
    }

    // TileHeight
    public static readonly DependencyProperty TileHeightProperty =
        DependencyProperty.Register(
            nameof(TileHeight),
            typeof(double),
            typeof(MonthTile),
            new PropertyMetadata(64.0));

    public double TileHeight
    {
        get => (double)GetValue(TileHeightProperty);
        set => SetValue(TileHeightProperty, value);
    }
}
