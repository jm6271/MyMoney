using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Models
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
