using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Extensions;

namespace MyMoney.Themes
{
    public class AccentColor
    {
        public Color BaseColor { get; set; }

        public string Name { get; set; } = string.Empty;

        public SolidColorBrush BaseBrush => new(BaseColor);
    }
}
