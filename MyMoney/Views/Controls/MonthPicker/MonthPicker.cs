using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MyMoney.Views.Controls;

[TemplatePart(Name = "PART_ToggleButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
[TemplatePart(Name = "PART_PrevButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_NextButton", Type = typeof(Button))]
public class MonthPicker : Control
{
    // Static default values computed once at class load time
    private static readonly DateTime s_defaultMinDate =
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-24);
    private static readonly DateTime s_defaultMaxDate =
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(24);
    private static readonly DateTime s_defaultSelectedMonth =
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private static readonly Brush s_defaultAccentColor =
        new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));

    // ── MinDate ──────────────────────────────────────────────────────────────
    public static readonly DependencyProperty MinDateProperty =
        DependencyProperty.Register(
            nameof(MinDate),
            typeof(DateTime),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                s_defaultMinDate,
                OnMinDateChanged));

    public DateTime MinDate
    {
        get => (DateTime)GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    private static void OnMinDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (MonthPicker)d;
        // Normalize to day=1
        var newMin = (DateTime)e.NewValue;
        var normalized = new DateTime(newMin.Year, newMin.Month, 1);
        if (normalized != newMin)
        {
            picker.MinDate = normalized;
            return;
        }
        picker.RebuildTiles();
    }

    // ── MaxDate ──────────────────────────────────────────────────────────────
    public static readonly DependencyProperty MaxDateProperty =
        DependencyProperty.Register(
            nameof(MaxDate),
            typeof(DateTime),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                s_defaultMaxDate,
                OnMaxDateChanged));

    public DateTime MaxDate
    {
        get => (DateTime)GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    private static void OnMaxDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (MonthPicker)d;
        // Normalize to day=1
        var newMax = (DateTime)e.NewValue;
        var normalized = new DateTime(newMax.Year, newMax.Month, 1);
        if (normalized != newMax)
        {
            picker.MaxDate = normalized;
            return;
        }
        // Clamp: if MinDate > MaxDate, set MaxDate = MinDate
        if (picker.MinDate > normalized)
        {
            picker.MaxDate = picker.MinDate;
            return;
        }
        picker.RebuildTiles();
    }

    // ── SelectedMonth ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty SelectedMonthProperty =
        DependencyProperty.Register(
            nameof(SelectedMonth),
            typeof(DateTime),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                s_defaultSelectedMonth,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged,
                CoerceSelectedMonth));

    public DateTime SelectedMonth
    {
        get => (DateTime)GetValue(SelectedMonthProperty);
        set => SetValue(SelectedMonthProperty, value);
    }

    private static object CoerceSelectedMonth(DependencyObject d, object baseValue)
    {
        var picker = (MonthPicker)d;
        var value = (DateTime)baseValue;

        // Normalize day to 1
        var normalized = new DateTime(value.Year, value.Month, 1);

        // Clamp to [MinDate, MaxDate]
        if (normalized < picker.MinDate)
            return picker.MinDate;
        if (normalized > picker.MaxDate)
            return picker.MaxDate;

        return normalized;
    }

    private static void OnSelectedMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (MonthPicker)d;
        var newSelectedMonth = (DateTime)e.NewValue;

        // Update IsSelected flags in-place (no full rebuild)
        for (int i = 0; i < picker.Tiles.Count; i++)
        {
            picker.Tiles[i] = picker.Tiles[i] with { IsSelected = picker.Tiles[i].Month == newSelectedMonth };
        }

        // Raise the SelectedMonthChanged routed event with the new value
        picker.RaiseEvent(new SelectedMonthChangedEventArgs(SelectedMonthChangedEvent, picker, newSelectedMonth));

        // Execute SelectionChangedCommand if set and CanExecute returns true
        var command = picker.SelectionChangedCommand;
        if (command != null && command.CanExecute(newSelectedMonth))
            command.Execute(newSelectedMonth);
    }

    // ── SelectedMonthChanged routed event ────────────────────────────────────
    public static readonly RoutedEvent SelectedMonthChangedEvent =
        EventManager.RegisterRoutedEvent(
            "SelectedMonthChanged",
            RoutingStrategy.Bubble,
            typeof(EventHandler<SelectedMonthChangedEventArgs>),
            typeof(MonthPicker));

    public event EventHandler<SelectedMonthChangedEventArgs> SelectedMonthChanged
    {
        add => AddHandler(SelectedMonthChangedEvent, value);
        remove => RemoveHandler(SelectedMonthChangedEvent, value);
    }

    // ── SelectionChangedCommand ───────────────────────────────────────────────
    public static readonly DependencyProperty SelectionChangedCommandProperty =
        DependencyProperty.Register(
            nameof(SelectionChangedCommand),
            typeof(ICommand),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(null));

    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    // ── AccentColor ───────────────────────────────────────────────────────────
    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(
            nameof(AccentColor),
            typeof(Brush),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                s_defaultAccentColor,
                OnVisualPropChanged));

    public Brush AccentColor
    {
        get => (Brush)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    // ── TileWidth ─────────────────────────────────────────────────────────────
    public static readonly DependencyProperty TileWidthProperty =
        DependencyProperty.Register(
            nameof(TileWidth),
            typeof(double),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                80.0,
                OnVisualPropChanged));

    public double TileWidth
    {
        get => (double)GetValue(TileWidthProperty);
        set => SetValue(TileWidthProperty, value);
    }

    // ── TileHeight ────────────────────────────────────────────────────────────
    public static readonly DependencyProperty TileHeightProperty =
        DependencyProperty.Register(
            nameof(TileHeight),
            typeof(double),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                64.0,
                OnVisualPropChanged));

    public double TileHeight
    {
        get => (double)GetValue(TileHeightProperty);
        set => SetValue(TileHeightProperty, value);
    }

    // ── PopupMaxWidth ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty PopupMaxWidthProperty =
        DependencyProperty.Register(
            nameof(PopupMaxWidth),
            typeof(double),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                500.0,
                OnPopupMaxWidthChanged));

    public double PopupMaxWidth
    {
        get => (double)GetValue(PopupMaxWidthProperty);
        set => SetValue(PopupMaxWidthProperty, value);
    }

    private static void OnPopupMaxWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var v = (double)e.NewValue;
        if (v <= 0.0)
            ((MonthPicker)d).PopupMaxWidth = double.PositiveInfinity;
    }

    // ── PopupHorizontalOffset ─────────────────────────────────────────────────
    public static readonly DependencyProperty PopupHorizontalOffsetProperty =
        DependencyProperty.Register(
            nameof(PopupHorizontalOffset),
            typeof(double),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(0.0));

    public double PopupHorizontalOffset
    {
        get => (double)GetValue(PopupHorizontalOffsetProperty);
        set => SetValue(PopupHorizontalOffsetProperty, value);
    }

    private static void OnVisualPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MonthPicker)d).PropagateVisualProps();
    }

    static MonthPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(typeof(MonthPicker)));
    }

    public MonthPicker()
    {
        Tiles = new ObservableCollection<MonthTileData>();
        RebuildTiles();
    }

    // Template part fields
    private ToggleButton? _toggleButton;
    private Popup? _popup;
    private ScrollViewer? _scrollViewer;
    private Button? _prevButton;
    private Button? _nextButton;

    // Internal tile collection
    public ObservableCollection<MonthTileData> Tiles { get; }

    public override void OnApplyTemplate()
    {
        // Unwire old handlers before re-wiring (template may be re-applied)
        if (_popup != null)
            _popup.Opened -= OnPopupOpened;
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= OnScrollChanged;
            _scrollViewer.RemoveHandler(
                Button.ClickEvent,
                new RoutedEventHandler(OnTileButtonClick));
        }
        if (_prevButton != null)
            _prevButton.Click -= OnPrevButtonClick;
        if (_nextButton != null)
            _nextButton.Click -= OnNextButtonClick;

        base.OnApplyTemplate();

        _toggleButton = GetTemplateChild("PART_ToggleButton") as ToggleButton;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        _prevButton = GetTemplateChild("PART_PrevButton") as Button;
        _nextButton = GetTemplateChild("PART_NextButton") as Button;

        if (_popup != null)
        {
            _popup.Opened += OnPopupOpened;
            _popup.Placement = PlacementMode.Custom;
            _popup.CustomPopupPlacementCallback = PlacePopupCentered;
        }
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
            _scrollViewer.AddHandler(
                Button.ClickEvent,
                new RoutedEventHandler(OnTileButtonClick));
        }
        if (_prevButton != null)
            _prevButton.Click += OnPrevButtonClick;
        if (_nextButton != null)
            _nextButton.Click += OnNextButtonClick;
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        ScrollToSelected();
    }

    private CustomPopupPlacement[] PlacePopupCentered(Size popupSize, Size targetSize, Point offset)
    {
        // Center the popup under the button, then apply the user's offset
        double x = (targetSize.Width - popupSize.Width) / 2.0 + PopupHorizontalOffset;
        double y = targetSize.Height; // directly below the button
        return [new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Horizontal)];
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        UpdateNavButtons();
    }

    private void OnTileButtonClick(object sender, RoutedEventArgs e)
    {
        // Walk up the visual tree from the original source to find a MonthTileData context
        var source = e.OriginalSource as DependencyObject;
        while (source != null)
        {
            if (source is FrameworkElement fe && fe.DataContext is MonthTileData tileData)
            {
                OnTileClicked(tileData.Month);
                e.Handled = true;
                return;
            }
            source = source is Visual || source is Visual3D
                ? VisualTreeHelper.GetParent(source)
                : LogicalTreeHelper.GetParent(source);
        }
    }

    private void OnPrevButtonClick(object sender, RoutedEventArgs e)
    {
        _scrollViewer?.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset - TileWidth);
    }

    private void OnNextButtonClick(object sender, RoutedEventArgs e)
    {
        _scrollViewer?.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + TileWidth);
    }

    private void RebuildTiles()
    {
        Tiles.Clear();

        var min = MinDate;
        var max = MaxDate;

        // Defensive clamp: if MinDate > MaxDate, treat MaxDate as MinDate
        if (min > max)
            max = min;

        var today = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var current = min;
        while (current <= max)
        {
            Tiles.Add(new MonthTileData(
                Month: current,
                IsSelected: false,
                IsCurrentMonth: current == today,
                AccentColor: AccentColor,
                TileWidth: TileWidth,
                TileHeight: TileHeight));

            current = current.AddMonths(1);
        }

        // Re-coerce SelectedMonth so it stays valid after range changes,
        // and update IsSelected flags accordingly.
        CoerceValue(SelectedMonthProperty);
    }

    private void PropagateVisualProps()
    {
        var accent = AccentColor;
        var width = TileWidth;
        var height = TileHeight;
        for (int i = 0; i < Tiles.Count; i++)
            Tiles[i] = Tiles[i] with { AccentColor = accent, TileWidth = width, TileHeight = height };
    }

    private void ScrollToSelected()
    {
        if (_scrollViewer == null) return;

        var tileIndex = -1;
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i].Month == SelectedMonth)
            {
                tileIndex = i;
                break;
            }
        }

        if (tileIndex < 0) return;

        var offset = tileIndex * TileWidth - _scrollViewer.ViewportWidth / 2.0 + TileWidth / 2.0;
        offset = Math.Max(0, Math.Min(offset, _scrollViewer.ScrollableWidth));
        _scrollViewer.ScrollToHorizontalOffset(offset);
    }

    private void UpdateNavButtons()
    {
        if (_scrollViewer == null || _prevButton == null || _nextButton == null) return;

        _prevButton.IsEnabled = _scrollViewer.HorizontalOffset > 0;
        _nextButton.IsEnabled = _scrollViewer.HorizontalOffset < _scrollViewer.ScrollableWidth;
    }

    private void OnTileClicked(DateTime month)
    {
        // Normalize to day=1 and set SelectedMonth
        SelectedMonth = new DateTime(month.Year, month.Month, 1);

        // SelectedMonthChanged event raised in OnSelectedMonthChanged callback (task 5.1)
        // SelectionChangedCommand executed in OnSelectedMonthChanged callback (task 5.3)

        // Close the popup after selection
        if (_popup != null)
            _popup.IsOpen = false;
    }
}
