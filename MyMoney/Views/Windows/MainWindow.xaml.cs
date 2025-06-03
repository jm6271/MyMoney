using MyMoney.Core.Database;
using MyMoney.Helpers.RadioButtonConverters;
using MyMoney.ViewModels.Pages;
using MyMoney.ViewModels.Windows;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider pageService,
            INavigationService navigationService,
            IContentDialogService contentDialogService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(pageService);

            navigationService.SetNavigationControl(RootNavigation);

            contentDialogService.SetDialogHost(RootContentDialog);

            // Load the theme from settings
            Core.Database.DatabaseReader dbReader = new();
            var SettingsDict = dbReader.GetSettingsDictionary("ApplicationSettings");

            if (SettingsDict != null && SettingsDict.Count > 0 && SettingsDict.ContainsKey("AppTheme"))
            {
                if (SettingsDict["AppTheme"] == "Light")
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);

                    // Set the color dynamic resources
                    Application.Current.Resources["LayerFillColorDefaultColor"] = Application.Current.Resources["LayerFillColorDefaultColorLight"];
                }
                else
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);

                    // set the color dynamic resources
                    Application.Current.Resources["LayerFillColorDefaultColor"] = Application.Current.Resources["LayerFillColorDefaultColorDark"];
                }
                    
            }
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check settings to see if we need to do a backup
            var databaseReader = new DatabaseReader();
            var settingsDict = databaseReader.GetSettingsDictionary("ApplicationSettings");

            BackupModeRadioButtonGroup BackupMode = BackupModeRadioButtonGroup.Manual;
            string BackupLocation = "";
            SettingsViewModel.BackupStorageDuration BackupStorageDuration = SettingsViewModel.BackupStorageDuration.OneWeek;

            // Extract the settings values
            if (settingsDict.TryGetValue("BackupMode", out string? backupMode))
            {
                try
                {
                    BackupMode = (BackupModeRadioButtonGroup)Convert.ToInt32(backupMode);
                }
                catch { /* If we can't load a setting, ignore it */ }
            }

            if (settingsDict.TryGetValue("BackupLocation", out string? backupLocation))
            {
                BackupLocation = backupLocation;
            }

            if (settingsDict.TryGetValue("BackupStorageDuration", out string? backupStorageDuration))
            {
                try
                {
                    BackupStorageDuration = (SettingsViewModel.BackupStorageDuration)Convert.ToInt32(backupStorageDuration);
                }
                catch { /* If we can't load a setting, ignore it */ }
            }

            // Write backup
            if (BackupMode == BackupModeRadioButtonGroup.Automatic && backupLocation != "")
            {
                // Backup
                DatabaseBackup.WriteDatabaseBackup(Path.Combine(BackupLocation, $"mymoney-database-backup-{DateTime.Now.ToString("MM-dd-yyyy")}.db"));

                // Clear any old backups

                // Get all *.db files in the backup directory
                var filesInBackupDir = Directory.EnumerateFiles(BackupLocation, "*.db");
                var regex = new Regex(@"mymoney-database-backup-(\d{2})-(\d{2})-(\d{4})\.db");

                foreach (var file in filesInBackupDir)
                {
                    var filename = Path.GetFileName(file);
                    var match = regex.Match(filename);
                    if (match.Success)
                    {
                        var month = int.Parse(match.Groups[1].Value);
                        var day = int.Parse(match.Groups[2].Value);
                        var year = int.Parse(match.Groups[3].Value);

                        if (DateTime.TryParseExact(
                            $"{year}-{month:D2}-{day:D2}",
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime backupDate))
                        {
                            int daysToKeep = 0;

                            switch (BackupStorageDuration)
                            {
                                case SettingsViewModel.BackupStorageDuration.FourDays:
                                    daysToKeep = 4;
                                    break;
                                case SettingsViewModel.BackupStorageDuration.OneWeek:
                                    daysToKeep = 7;
                                    break;
                                case SettingsViewModel.BackupStorageDuration.TwoWeeks:
                                    daysToKeep = 14;
                                    break;
                                case SettingsViewModel.BackupStorageDuration.OneMonth:
                                    daysToKeep = 30;
                                    break;
                                case SettingsViewModel.BackupStorageDuration.ThreeMonths:
                                    daysToKeep = 90;
                                    break;
                                case SettingsViewModel.BackupStorageDuration.Forever:
                                    return;
                                default:
                                    break;
                            }

                            if (backupDate < DateTime.Today.AddDays(-daysToKeep))
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch { /* Fail silently, it's not important that the file is deleted*/ }
                                }
                            }
                        }
                    }
            }
        }
    }
}
