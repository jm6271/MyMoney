﻿using Microsoft.Win32;
using MyMoney.Core.Database;
using MyMoney.Helpers.RadioButtonConverters;
using MyMoney.Services.ContentDialogs;
using System.Reflection;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyMoney.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for the settings page, handling application configuration and backup management
    /// </summary>
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        #region Settings Properties

        /// <summary>
        /// Current backup mode setting
        /// </summary>
        [ObservableProperty]
        private BackupModeRadioButtonGroup _backupMode = BackupModeRadioButtonGroup.Manual;

        /// <summary>
        /// Location for automatic backups
        /// </summary>
        [ObservableProperty]
        private string _backupLocation = string.Empty;

        /// <summary>
        /// Current application theme
        /// </summary>
        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        /// <summary>
        /// Application version display string
        /// </summary>
        [ObservableProperty]
        private string _appVersion = string.Empty;

        /// <summary>
        /// Duration for storing backups
        /// </summary>
        [ObservableProperty]
        private BackupStorageDuration _backupDuration = BackupStorageDuration.OneWeek;

        #endregion

        #region State Management

        private bool _isInitialized;
        private bool _backupSettingsLoaded;
        private readonly IMessageBoxService _messageBoxService;
        private const string SettingsKey = "ApplicationSettings";

        #endregion

        #region Public Properties

        /// <summary>
        /// Index for backup duration combobox
        /// </summary>
        public int BackupDurationIndex
        {
            get => (int)BackupDuration;
            set
            {
                BackupDuration = (BackupStorageDuration)value;
                OnPropertyChanged(nameof(BackupDurationIndex));
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// Available durations for storing backups
        /// </summary>
        public enum BackupStorageDuration
        {
            FourDays = 0,
            OneWeek = 1,
            TwoWeeks = 2,
            OneMonth = 3,
            ThreeMonths = 4,
            Forever = 5,
        }

        #endregion

        #region Constructor

        public SettingsViewModel(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        }

        #endregion

        #region Navigation Methods

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        #endregion

        #region Initialization

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            LoadBackupSettings();
            AppVersion = GetVersionString();
            _isInitialized = true;
        }

        private static string GetVersionString()
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion?.Split('+')[0];
            return version ?? "";
        }

        #endregion

        #region Theme Management

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            var newTheme = parameter switch
            {
                "theme_light" when CurrentTheme != ApplicationTheme.Light => ApplicationTheme.Light,
                _ when CurrentTheme != ApplicationTheme.Dark => ApplicationTheme.Dark,
                _ => CurrentTheme
            };

            if (newTheme == CurrentTheme) return;

            ApplyTheme(newTheme);
            SaveTheme();
        }

        private void ApplyTheme(ApplicationTheme theme)
        {
            ApplicationThemeManager.Apply(theme);
            CurrentTheme = theme;
            
            var resourceKey = theme == ApplicationTheme.Light 
                ? "LayerFillColorDefaultColorLight" 
                : "LayerFillColorDefaultColorDark";

            Application.Current.Resources["LayerFillColorDefaultColor"] = 
                Application.Current.Resources[resourceKey];
        }

        private void SaveTheme()
        {
            var settings = GetSettings();
            settings["AppTheme"] = CurrentTheme == ApplicationTheme.Light ? "Light" : "Dark";
            SaveSettings(settings);
        }

        #endregion

        #region Backup Management

        [RelayCommand]
        private void SaveBackupSettings()
        {
            if (!_backupSettingsLoaded) return;

            var settings = GetSettings();
            settings["BackupMode"] = ((int)BackupMode).ToString();
            settings["BackupLocation"] = BackupLocation;
            settings["BackupStorageDuration"] = BackupDurationIndex.ToString();
            
            SaveSettings(settings);
        }

        private void LoadBackupSettings()
        {
            var settings = GetSettings();

            if (settings.TryGetValue("BackupMode", out var backupMode))
                TrySetBackupMode(backupMode);

            if (settings.TryGetValue("BackupLocation", out var location))
                BackupLocation = location;

            if (settings.TryGetValue("BackupStorageDuration", out var duration))
                TrySetBackupDuration(duration);

            _backupSettingsLoaded = true;
        }

        private void TrySetBackupMode(string value)
        {
            if (int.TryParse(value, out var mode) && 
                Enum.IsDefined(typeof(BackupModeRadioButtonGroup), mode))
            {
                BackupMode = (BackupModeRadioButtonGroup)mode;
            }
        }

        private void TrySetBackupDuration(string value)
        {
            if (int.TryParse(value, out var duration) && 
                Enum.IsDefined(typeof(BackupStorageDuration), duration))
            {
                BackupDurationIndex = duration;
            }
        }

        [RelayCommand]
        private void BackupNow()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "MyMoney Databases|*.db",
                Title = "Choose backup location...",
                FileName = $"mymoney-backup-{DateTime.Now:MM-dd-yyyy-HH_mm}.db"
            };

            if (dialog.ShowDialog() == true)
            {
                DatabaseBackup.WriteDatabaseBackup(dialog.FileName);
            }
        }

        [RelayCommand]
        private async Task RestoreFromBackup()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "MyMoney Databases|*.db",
                Title = "Choose backup file to restore from..."
            };

            if (dialog.ShowDialog() != true) return;

            var confirmed = await _messageBoxService.ShowAsync(
                "Really Backup?", 
                "Are you sure you want to restore the backup? This will OVERWRITE all of your data and replace it with the data in the backup.",
                "Yes", 
                "No");

            if (confirmed != Wpf.Ui.Controls.MessageBoxResult.Primary) return;

            DatabaseBackup.RestoreDatabaseBackup(dialog.FileName);

            await _messageBoxService.ShowInfoAsync(
                "Restore Successful",
                "The backup was restored successfully. The application will now close.",
                "Restart");

            Application.Current.Shutdown();
        }

        [RelayCommand]
        private void ChooseAutomaticBackupLocation()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                BackupLocation = dialog.FolderName;
                SaveBackupSettings();
            }
        }

        #endregion

        #region Settings Helper Methods

        private static Dictionary<string, string> GetSettings()
        {
            var reader = new DatabaseManager();
            return reader.GetSettingsDictionary(SettingsKey);
        }

        private static void SaveSettings(Dictionary<string, string> settings)
        {
            var writer = new DatabaseManager();
            writer.WriteSettingsDictionary(SettingsKey, settings);
        }

        #endregion
    }
}
