using LiteDB;
using MyMoney.Core.Models;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MyMoney.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"MyMoney - {GetAssemblyVersion()}";

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
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

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }

            SaveTheme();
        }

        private void SaveTheme()
        {
            using (var db = new LiteDatabase(Helpers.DataFileLocationGetter.GetDataFilePath()))
            {
                var SettingsCollection = db.GetCollection<SettingsModel>("AppSettings");
                bool ThemeSaved = false;

                for (int i = 1; i <= SettingsCollection.Count(); i++)
                {
                    SettingsModel setting = SettingsCollection.FindById(i);
                    if (setting.SettingsKey == "AppTheme")
                    {
                        if (CurrentTheme == ApplicationTheme.Light)
                            setting.SettingsValue = "Light";
                        else if (CurrentTheme == ApplicationTheme.Dark)
                            setting.SettingsValue = "Dark";
                        ThemeSaved = true;
                        break;
                    }
                }

                if (!ThemeSaved)
                {
                    // Theme did not exist in the database before, we'll have to create it
                    SettingsModel theme = new();
                    theme.SettingsKey = "AppTheme";

                    if (CurrentTheme == ApplicationTheme.Light)
                        theme.SettingsValue = "Light";
                    else if (CurrentTheme == ApplicationTheme.Dark)
                        theme.SettingsValue = "Dark";

                    SettingsCollection.Insert(theme);
                }
            }
        }
    }
}
