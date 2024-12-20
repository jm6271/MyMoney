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

        private static string GetAssemblyVersion()
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


#pragma warning disable WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    Application.Current.ThemeMode = ThemeMode.Light;
#pragma warning restore WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    
                    
                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

#pragma warning disable WPF0001
                    Application.Current.ThemeMode = ThemeMode.Dark;
#pragma warning restore WPF0001

                    break;
            }

            SaveTheme();
        }

        private void SaveTheme()
        {
            var SettingsDict = Core.Database.DatabaseReader.GetSettingsDictionary("ApplicationSettings");

            // Assign the theme
            if (CurrentTheme == ApplicationTheme.Light)
                SettingsDict["AppTheme"] = "Light";
            else
                SettingsDict["AppTheme"] = "Dark";

            // Write to database
            Core.Database.DatabaseWriter.WriteSettingsDictionary("ApplicationSettings", SettingsDict);
        }
    }
}
