﻿using MyMoney.ViewModels.Windows;
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
    }
}
