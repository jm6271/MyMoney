using Wpf.Ui.Appearance;
using Wpf.Ui.Abstractions.Controls;
using MyMoney.Core.Database;

namespace MyMoney.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized;

        [ObservableProperty]
        private string _appVersion = string.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
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
    }
}
