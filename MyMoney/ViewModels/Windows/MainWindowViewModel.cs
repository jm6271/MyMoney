﻿using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace MyMoney.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "MyMoney";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Accounts",
                Icon = new SymbolIcon { Symbol = SymbolRegular.TextBulletListRtl24 },
                TargetPageType = typeof(Views.Pages.AccountsPage)
            },
            new NavigationViewItem()
            {
                Content = "Budget",
                Icon = new SymbolIcon { Symbol = SymbolRegular.MoneyCalculator24 },
                TargetPageType = typeof(Views.Pages.BudgetPage)
            },
            new NavigationViewItem()
            {
                Content = "Reports",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ChartMultiple24 },
                TargetPageType= typeof(Views.Pages.ReportsPage)
            },
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
