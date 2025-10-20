using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
using JAGLog;
using WinUINotifsMSIX.ViewModels;
using WinUINotifsMSIX.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUINotifsMSIX
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private DisplayRequest? _displayRequest;
        private int _displayRefCount;
        public const string BettingConnectionString = "Server=.\\SQLEXPRESS;database=Betting;Integrated Security=SSPI;MultipleActiveResultSets=True;TrustServerCertificate=True";
        bool DoDisplayRequest = false; // when true the system seems to never sleep. sleep handling now done in PowerStateMonitor
        private NotificationsViewModel? _viewModel;
        public App()
        {
            InitializeComponent();

            JAGLogExtensions.ConfigureSerilog(BettingConnectionString);
            Log.Information("ConfigureSerilog complete");

            // Subscribe to platform lifecycle events via CoreApplication
            CoreApplication.Suspending += CoreApplication_Suspending;
            CoreApplication.Resuming += CoreApplication_Resuming;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _ = new LogMaintenanceService().TriggerHousekeepingAsync();

            _window = new MainWindow();
            _window.Activated += Window_Activated;
            _window.Closed += Window_Closed;
            _window.Activate();
            if (_window is MainWindow mw)
            {
                _viewModel = mw._viewModel;
            }
            if (DoDisplayRequest)
            {
                RequestDisplay();
            }

        }
        public void RequestWindowFocus()
        {
            if (_window is MainWindow mw)
            {
                mw.FlashTaskbarIcon(); // only works when not focused
                mw.BringToForeground();
                mw.ForceFlashTaskbarIcon(); // works even when focused in theory but didnt seem to work
            }
        }
        private void Window_Activated(object? sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != WindowActivationState.Deactivated)
            {
                if (DoDisplayRequest)
                {
                    RequestDisplay();
                }
            }
            else
            {
                // Optionally release when deactivated:
                // ReleaseDisplay();
            }
        }

        private void Window_Closed(object? sender, WindowEventArgs e)
        {
            if (DoDisplayRequest)
            {
                ReleaseDisplay();
            }

        }

        private void RequestDisplay()
        {
            try
            {
                if (_displayRequest == null) _displayRequest = new DisplayRequest();
                _displayRequest.RequestActive();
                _displayRefCount++;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "RequestDisplay failed");
                //System.Diagnostics.Debug.WriteLine($"RequestDisplay failed: {ex}");
            }
        }

        private void ReleaseDisplay()
        {
            try
            {
                if (_displayRequest == null || _displayRefCount <= 0) return;
                _displayRequest.RequestRelease();
                _displayRefCount--;
                if (_displayRefCount <= 0)
                {
                    _displayRequest = null;
                    _displayRefCount = 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ReleaseDisplay failed");
                //System.Diagnostics.Debug.WriteLine($"ReleaseDisplay failed: {ex}");
            }
        }

        // CoreApplication Suspending handler
        private void CoreApplication_Suspending(object? sender, SuspendingEventArgs e)
        {
            // Release the display lock on suspend
            if (DoDisplayRequest)
            {
                ReleaseDisplay();
            }
            // Optionally get a deferral for async work:
            // var def = e.SuspendingOperation.GetDeferral();
            // try { /* async cleanup */ } finally { def.Complete(); }
            _viewModel?.StopLogTimer();

        }

        // CoreApplication Resuming handler
        private void CoreApplication_Resuming(object? sender, object e)
        {
            // Re-request display if appropriate
            if (DoDisplayRequest)
            {
                RequestDisplay();
            }
            _viewModel?.StartLogTimer();

        }
    }

}
