using System.Windows.Media;

namespace MyMoney.Themes
{
    public static class PresetAccentColors
    {
        public static List<AccentColor> AccentColors { get; } =
            new()
            {
                new() { Name = "Blue", BaseColor = Color.FromArgb(0xFF, 0x00, 0x78, 0xD7) },
                new() { Name = "Deep Blue", BaseColor = Color.FromArgb(0xFF, 0x25, 0x63, 0xEB) },
                new() { Name = "Soft Blue", BaseColor = Color.FromArgb(0xFF, 0x60, 0xA5, 0xFA) },
                new() { Name = "Cyan Blue", BaseColor = Color.FromArgb(0xFF, 0x00, 0x99, 0xBC) },
                new() { Name = "Teal", BaseColor = Color.FromArgb(0xFF, 0x00, 0xB7, 0xC3) },
                new() { Name = "Lime Green", BaseColor = Color.FromArgb(0xFF, 0x10, 0x7C, 0x10) },
                new() { Name = "Fresh Green", BaseColor = Color.FromArgb(0xFF, 0x16, 0xA3, 0x4A) },
                new() { Name = "Mint Green", BaseColor = Color.FromArgb(0xFF, 0x4A, 0xDE, 0x80) },
                new() { Name = "Violet", BaseColor = Color.FromArgb(0xFF, 0x8B, 0x5C, 0xF6) },
                new() { Name = "Deep Purple", BaseColor = Color.FromArgb(0xFF, 0x7C, 0x3A, 0xED) },
            };
    }
}
