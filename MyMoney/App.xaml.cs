using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMoney.Core.Database;
using MyMoney.Services;
using MyMoney.Services.ContentDialogs;
using MyMoney.ViewModels.Pages;
using MyMoney.ViewModels.Pages.ReportPages;
using MyMoney.ViewModels.Windows;
using MyMoney.Views.Pages;
using MyMoney.Views.Pages.ReportPages;
using MyMoney.Views.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace MyMoney
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => 
                { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) 
                                ?? throw new InvalidOperationException()); })
            .ConfigureServices((context, services) =>
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
                services.AddSingleton<AccountsPage>();
                services.AddSingleton<AccountsViewModel>();
                services.AddSingleton<BudgetPage>();
                services.AddSingleton<BudgetViewModel>();
                services.AddSingleton<ReportsPage>();
                services.AddSingleton<ReportsViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();

                // Report pages
                services.AddSingleton<BudgetReportsPage>();
                services.AddSingleton<BudgetReportsViewModel>();

                // Database
                services.AddSingleton<IDatabaseReader, DatabaseReader>();

                // Custom content dialogs
                services.AddSingleton<INewAccountDialogService, NewAccountDialogService>();
                services.AddSingleton<ITransferDialogService, TransferDialogService>();
                services.AddSingleton<ITransactionDialogService, TransactionDialogService>();
                services.AddSingleton<IRenameAccountDialogService, RenameAccountDialogService>();
                services.AddSingleton<IMessageBoxService, MessageBoxService>();

            }).Build();

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
            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
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
