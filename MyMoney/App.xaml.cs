using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMoney.Core.Database;
using MyMoney.Services;
using MyMoney.ViewModels.Pages;
using MyMoney.ViewModels.Windows;
using MyMoney.Views.Pages;
using MyMoney.Views.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace MyMoney
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // Mutex object to ensure single instance application
        private static Mutex? _mutex;

        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)
                        ?? throw new InvalidOperationException()
                );
            })
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddHostedService<ApplicationHostService>();

                    // Page resolver service
                    services.AddSingleton<INavigationViewPageProvider, PageService>();

                    // Theme manipulation
                    services.AddSingleton<IThemeService, ThemeService>();

                    // TaskBar manipulation
                    services.AddSingleton<ITaskBarService, TaskBarService>();

                    // Service containing navigation, same as INavigationWindow... but without window
                    services.AddSingleton<INavigationService, NavigationService>();

                    // Service for showing content dialogs
                    services.AddSingleton<IContentDialogService, ContentDialogService>();

                    // Main window with navigation
                    services.AddSingleton<INavigationWindow, MainWindow>();
                    services.AddSingleton<MainWindowViewModel>();

                    services.AddSingleton<DashboardPage>();
                    services.AddSingleton<DashboardViewModel>();
                    services.AddTransient<AccountsPage>();
                    services.AddTransient<AccountsViewModel>();
                    services.AddSingleton<BudgetPage>();
                    services.AddSingleton<BudgetViewModel>();
                    services.AddTransient<ReportsPage>();
                    services.AddTransient<ReportsViewModel>();
                    services.AddSingleton<SettingsPage>();
                    services.AddSingleton<SettingsViewModel>();

                    // Report pages
                    services.AddTransient<BudgetReportsPage>();
                    services.AddTransient<BudgetReportsViewModel>();

                    // Database
                    services.AddSingleton<IDatabaseManager, DatabaseManager>();

                    services.AddSingleton<IContentDialogFactory, ContentDialogFactory>();

                    services.AddTransient<IMessageBoxService, MessageBoxService>();
                }
            )
            .Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Ensure single instance application using mutex
            _mutex = new Mutex(true, "MyMoney-DB684A92-F90C-4814-B3DF-F92C1615F21E", out bool isNewInstance);
            if (!isNewInstance)
            {
                // Application is already running
                MessageBox.Show(
                    "Another instance of the application is already running.",
                    "MyMoney",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                Shutdown();
                return;
            }
            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();

            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
