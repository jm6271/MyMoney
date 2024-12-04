using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models
{
    public partial class SettingsModel : ObservableObject
    {
        [ObservableProperty]
        private string _SettingsKey = "";

        [ObservableProperty]
        private string _SettingsValue = "";

        [ObservableProperty]
        private int _Id = 0;
    }
}
