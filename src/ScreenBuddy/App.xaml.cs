using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScreenBuddy.Application;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Messages;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Timer;
using ScreenBuddy.Infrastructure.Logging;
using ScreenBuddy.Infrastructure.Persistence;
using ScreenBuddy.Infrastructure.Platform;
using ScreenBuddy.Presentation.FirstRun;
using ScreenBuddy.Presentation.Main;
using ScreenBuddy.Presentation.Overlay;
using ScreenBuddy.Presentation.Tray;

namespace ScreenBuddy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Primary composition root and application lifecycle coordinator for ScreenBuddy.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static Mutex? _mutex;
        private static SingleInstanceService? _singleInstanceService;
        private const string MutexName = "ScreenBuddy_SingleInstance_Mutex_9A8B7C";

        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            // 1. Single Instance Mutex Guard & IPC Activation Signaling
            _mutex = new Mutex(true, MutexName, out bool isFirstInstance);
            if (!isFirstInstance)
            {
                SingleInstanceService.SignalRunningInstance();
                Shutdown();
                return;
            }

            base.OnStartup(e);

            // 2. Register Exception Handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // 3. Build Service Collection
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // 4. Initialize Core Infrastructure & Overlay Services
            var mainWindowService = ServiceProvider.GetRequiredService<IMainWindowService>();
            var trayService = ServiceProvider.GetRequiredService<ITrayService>();
            trayService.Initialize();

            // Start Single-Instance IPC Listener
            _singleInstanceService = new SingleInstanceService();
            _singleInstanceService.StartListening(() =>
            {
                mainWindowService.BringToForeground();
            });

            var overlayManager = ServiceProvider.GetRequiredService<IOverlayManager>();
            var sessionCoordinator = ServiceProvider.GetRequiredService<ISessionCoordinator>();
            var timerEngine = ServiceProvider.GetRequiredService<ITimerEngine>();
            var statePersister = ServiceProvider.GetRequiredService<ITimerStatePersister>();
            var powerListener = ServiceProvider.GetRequiredService<IPowerEventListener>();

            // Wire State Snapshot Persistence
            timerEngine.PersistStateRequested += (sender, snapshot) =>
            {
                statePersister.Write(snapshot);
            };

            // Wire Sleep/Wake Power Event Listener
            powerListener.SystemResuming += (sender, args) =>
            {
                var snapshot = statePersister.Read();
                sessionCoordinator.HandleWakeFromSleep(snapshot);
            };

            // Wire live tray tooltip remaining countdown display
            timerEngine.TimerTick += (sender, remainingSecs) =>
            {
                var trayVm = ServiceProvider.GetRequiredService<TrayViewModel>();
                trayVm.UpdateRemainingTime(remainingSecs);
            };

            // 5. First Run vs Recovery vs Normal Startup Flow
            string appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ScreenBuddy");
            string configFile = Path.Combine(appDataDir, "config.json");

            if (!File.Exists(configFile))
            {
                // Fresh Install: Show First Run onboarding
                var settingsService = ServiceProvider.GetRequiredService<ISettingsService>();
                var startupRegistrar = ServiceProvider.GetRequiredService<IStartupRegistrar>();
                var firstRunVm = new FirstRunViewModel(settingsService, sessionCoordinator, startupRegistrar);
                var firstRunWin = new FirstRunWindow(firstRunVm);
                firstRunWin.Show();
            }
            else
            {
                // Existing Install: Check Crash Recovery Snapshot
                var recoverySnapshot = statePersister.Read();
                if (recoverySnapshot != null && recoverySnapshot.Phase != SessionState.Stopped)
                {
                    sessionCoordinator.HandleWakeFromSleep(recoverySnapshot);
                }

                // Launch MainWindow in Ready/Current state (Does NOT auto-start work session on Windows boot)
                mainWindowService.Show();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddProvider(new FileLoggerProvider());
            });

            // Domain Services
            services.AddSingleton<IClock, ClockAdapter>();
            services.AddSingleton<IConfigPersister, ConfigPersister>();
            services.AddSingleton<ITimerStatePersister, TimerStatePersister>();
            services.AddSingleton<IMessageIndexPersister, MessageIndexPersister>();
            services.AddSingleton<IStartupRegistrar, StartupRegistrar>();
            services.AddSingleton<IPowerEventListener, PowerEventListener>();
            services.AddSingleton<IDisplayMonitor, DisplayMonitor>();

            // Message Library
            services.AddSingleton<IMessageLibrary>(sp =>
            {
                var persister = sp.GetRequiredService<IMessageIndexPersister>();
                var messages = LoadEmbeddedBreakMessages();
                return new MessageLibrary(messages, persister);
            });

            // Timer Engine & Application Coordinator
            services.AddSingleton<ITimerEngine, TimerEngine>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ISessionCoordinator, SessionCoordinator>();

            // Presentation Services & ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddSingleton<IMainWindowService>(sp =>
            {
                return new MainWindowService(() => sp.GetRequiredService<MainWindow>());
            });

            services.AddSingleton<TrayViewModel>();
            services.AddSingleton<ITrayService, TrayService>();
            services.AddSingleton<IOverlayManager, OverlayManager>();
        }

        private static List<string> LoadEmbeddedBreakMessages()
        {
            try
            {
                var asm = typeof(App).Assembly;
                using Stream? stream = asm.GetManifestResourceStream("ScreenBuddy.Resources.Messages.break-messages.json");
                if (stream != null)
                {
                    using StreamReader reader = new StreamReader(stream);
                    string json = reader.ReadToEnd();
                    var list = JsonSerializer.Deserialize<List<string>>(json);
                    if (list != null && list.Count > 0)
                    {
                        return list;
                    }
                }
            }
            catch
            {
                // Fallback
            }

            return new List<string>
            {
                "Rest is not the opposite of productivity. It is its fuel.",
                "Step away from the screen for a moment.",
                "Look into the distance to rest your eyes.",
                "Take a deep breath and let your shoulders drop.",
                "Unclamp your jaw and release your posture."
            };
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = ServiceProvider?.GetService<ILogger<App>>();
            logger?.LogError(e.Exception, "Unhandled UI Dispatcher Exception caught.");
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = ServiceProvider?.GetService<ILogger<App>>();
            if (e.ExceptionObject is Exception ex)
            {
                logger?.LogCritical(ex, "Unhandled AppDomain Exception caught.");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Delete timer recovery state on clean exit
                var statePersister = ServiceProvider?.GetService<ITimerStatePersister>();
                statePersister?.Delete();
            }
            catch
            {
                // Fail-safe
            }

            _singleInstanceService?.Dispose();

            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }

            if (ServiceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }

            base.OnExit(e);
        }
    }
}
