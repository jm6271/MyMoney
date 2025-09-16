using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace MyMoney.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "MyMoney";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems =
        [
            new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage),
                
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

            new NavigationViewItemSeparator(),

            new NavigationViewItem()
            {
                 Content = "Charts",
                 Icon = new SymbolIcon { Symbol = SymbolRegular.ChartMultiple24 },
                 TargetPageType = typeof(Views.Pages.ReportsPage),
            },

            new NavigationViewItem()
            {
                Content = "Budget Reports",
                Icon = new SymbolIcon { Symbol = SymbolRegular.BookCoins24 },
                TargetPageType = typeof(Views.Pages.BudgetReportsPage),
            },
        ];

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems =
        [
            new NavigationViewItem()
            {
                Content = "Help",
                Icon = new SymbolIcon { Symbol = SymbolRegular.QuestionCircle24 },
                Command = new RelayCommand(async () =>
                {
                    if (!System.IO.File.Exists(@"site\index.html"))
                    {
                        Wpf.Ui.Controls.MessageBox msgBox = new()
                        {
                            Title = "Help files not installed",
                            Content = "The help files are not installed.",
                        };
                        await msgBox.ShowDialogAsync();
                        return;
                    }

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(@"site\index.html")
                    {
                        UseShellExecute = true,
                    });
                })
            },
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        ];

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = [new MenuItem { Header = "Home", Tag = "tray_home" }];
    }
}
