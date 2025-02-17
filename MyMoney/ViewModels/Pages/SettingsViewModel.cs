﻿using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;
using MyMoney.Core.Database;

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
            var SettingsDict = reader.GetSettingsDictionary("ApplicationSettings");

            // Assign the theme
            if (CurrentTheme == ApplicationTheme.Light)
                SettingsDict["AppTheme"] = "Light";
            else
                SettingsDict["AppTheme"] = "Dark";

            // Write to database
            Core.Database.DatabaseWriter.WriteSettingsDictionary("ApplicationSettings", SettingsDict);
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
