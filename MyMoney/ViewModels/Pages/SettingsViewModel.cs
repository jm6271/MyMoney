using Microsoft.Win32;
using MyMoney.Core.Database;
using MyMoney.Helpers.RadioButtonConverters;
using MyMoney.Services.ContentDialogs;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyMoney.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private BackupModeRadioButtonGroup _backupMode = BackupModeRadioButtonGroup.Manual;

        [ObservableProperty]
        private string _backupLocation = "";

        private bool _isInitialized;

        private bool _backupSettingsLoaded = false;

        [ObservableProperty]
        private string _appVersion = string.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        public enum BackupStorageDuration
        {
            FourDays = 0,
            OneWeek = 1,
            TwoWeeks = 2,
            OneMonth = 3,
            ThreeMonths = 4,
            Forever = 5,
        }

        public int BackupDurationIndex
        {
            get
            {
                return (int)BackupDuration;
            }
            set
            {
                BackupDuration = (BackupStorageDuration)value;
                OnPropertyChanged(nameof(BackupDurationIndex));
            }
        }

        [ObservableProperty]
        private BackupStorageDuration _backupDuration = BackupStorageDuration.OneWeek;

        private readonly IMessageBoxService _messageBoxService;

        public SettingsViewModel(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
        }

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            LoadBackupSettings();
            AppVersion = $"MyMoney - {GetAssemblyVersion()}";

            _isInitialized = true;
        }

        private static string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? string.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;
                    Application.Current.Resources["LayerFillColorDefaultColor"] =
                        Application.Current.Resources["LayerFillColorDefaultColorLight"];
                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;
                    Application.Current.Resources["LayerFillColorDefaultColor"] =
                        Application.Current.Resources["LayerFillColorDefaultColorDark"];
                    break;
            }

            SaveTheme();
        }

        private void SaveTheme()
        {
            DatabaseReader reader = new();
            var settingsDict = reader.GetSettingsDictionary("ApplicationSettings");

            // Assign the theme
            if (CurrentTheme == ApplicationTheme.Light)
                settingsDict["AppTheme"] = "Light";
            else
                settingsDict["AppTheme"] = "Dark";

            // Write to database
            DatabaseWriter.WriteSettingsDictionary("ApplicationSettings", settingsDict);
        }

        [RelayCommand]
        private void SaveBackupSettings()
        {
            if (!_backupSettingsLoaded)
                return;
            DatabaseReader reader = new();
            var settingsDict = reader.GetSettingsDictionary("ApplicationSettings");

            // Save the settings
            settingsDict["BackupMode"] = ((int)BackupMode).ToString();
            settingsDict["BackupLocation"] = BackupLocation;
            settingsDict["BackupStorageDuration"] = BackupDurationIndex.ToString();

            // Write to database
            DatabaseWriter.WriteSettingsDictionary("ApplicationSettings", settingsDict);
        }

        private void LoadBackupSettings()
        {
            DatabaseReader reader = new();
            var settingsDict = reader.GetSettingsDictionary("ApplicationSettings");

            // Extract the settings values and load them
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
                    BackupDurationIndex = Convert.ToInt32(backupStorageDuration);
                }
                catch { /* If we can't load a setting, ignore it */ }
            }
            _backupSettingsLoaded = true;
        }

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void BackupNow()
        {
            // Show a save file dialog to get the location of the backup
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "MyMoney Databases|*.db";
            saveFileDialog.Title = "Choose backup location...";
            saveFileDialog.FileName = $"mymoney-backup-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.db";
            if (saveFileDialog.ShowDialog() == true)
            {
                DatabaseBackup.WriteDatabaseBackup(saveFileDialog.FileName);
            }
        }

        [RelayCommand]
        private async Task RestoreFromBackup()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "MyMoney Databases|*.db";
            openFileDialog.Title = "Choose backup file to restore from...";
            if (openFileDialog.ShowDialog() == true &&
                await _messageBoxService.ShowAsync("Really Backup?", "Are you sure you want to restore the backup?" +
                    " This will OVERWRITE all of you data and replace it with the data in the backup.",
                    "Yes", "No") == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                DatabaseBackup.RestoreDatabaseBackup(openFileDialog.FileName);

                await _messageBoxService.ShowInfoAsync("Restore Successful",
                    "The backup was restored successfully. The application will now close.",
                    "Restart");

                Application.Current.Shutdown();
            }
        }

        [RelayCommand]
        private void ChooseAutomaticBackupLocation()
        {
            OpenFolderDialog openFolderDialog = new();
            if (openFolderDialog.ShowDialog() == true)
            {
                BackupLocation = openFolderDialog.FolderName;
                SaveBackupSettings();
            }
        }
    }
}
