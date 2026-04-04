using System.Windows.Media;

namespace MyMoney.Views.Controls;

public record MonthTileData(
    DateTime Month,
    bool IsSelected,
    bool IsCurrentMonth,
    Brush AccentColor,
    double TileWidth,
    double TileHeight);
