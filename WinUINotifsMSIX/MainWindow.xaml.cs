using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using WinUINotifsMSIX.ViewModels;
using WinUINotifsMSIX.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUINotifsMSIX
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public NotificationsViewModel? _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            _ = InitializeAsync();
        }
        public void BringToForeground()
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
        }
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private async Task InitializeAsync()
        {
            try
            {

                var listener = UserNotificationListener.Current;

                // Request access first; VM will enumerate only if allowed
                var accessStatus = await listener.RequestAccessAsync();
                Log.Information("Notification listener access: {Status}", accessStatus);

                if (accessStatus != UserNotificationListenerAccessStatus.Allowed)
                {
                    Log.Information("Notification access not granted. Notifications will not be shown.");
                    // Still navigate to page so UI appears; VM will be null and page can show an error message if desired
                    NavigateToNotificationsPage(null);
                    return;
                }

                // Create ViewModel and navigate to the page
                _viewModel = new NotificationsViewModel(listener);
                _viewModel.PowerStateMonitor.IsSleepBlocked = true;
                _viewModel.AppVersion = GetAppVersion();

                NavigateToNotificationsPage(_viewModel);

                // initial load
                try
                {
                    await _viewModel.RefreshAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Initial RefreshAsync failed");
                }

                // Ensure we clean up when window closes
                this.Closed += MainWindow_Closed;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing notification listener or ViewModel");
                // navigate anyway so UI appears
                NavigateToNotificationsPage(null);
            }
        }
        public void FlashTaskbarIcon()
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            FLASHWINFO fwInfo = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = hwnd,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = uint.MaxValue,
                dwTimeout = 0
            };

            FlashWindowEx(ref fwInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        private const uint FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 12;

        public async void ForceFlashTaskbarIcon()
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            FLASHWINFO fwInfo = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = hwnd,
                dwFlags = FLASHW_ALL,
                uCount = 3, // Flash 3 times
                dwTimeout = 0
            };

            FlashWindowEx(ref fwInfo);

            await Task.Delay(1000); // Optional pause
            fwInfo.dwFlags = FLASHW_STOP;
            fwInfo.uCount = 0;
            FlashWindowEx(ref fwInfo);
        }

        private const uint FLASHW_STOP = 0;
        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
        private void NavigateToNotificationsPage(NotificationsViewModel? vm)
        {
            // Create the page and set its DataContext to the provided ViewModel (if any)
            var page = new Pages.NotificationsPage();
            if (vm != null)
            {
                page.DataContext = vm;
            }
            else
            {
                // Optionally set a minimal viewmodel or leave null; page can detect and show a helpful message
                page.DataContext = null;
            }

            // Navigate the Frame to the page instance
            // Note: using Navigate(Type) won't set the instance DataContext we just assigned,
            // so we directly set Frame.Content to the page instance.
            AppFrame.Content = page;
        }

        private void MainWindow_Closed(object? sender, WindowEventArgs e)
        {
            try
            {
                _viewModel.PowerStateMonitor.IsSleepBlocked = true;
                _viewModel?.Dispose();
                _viewModel = null;
                Log.Information("NotificationsViewModel disposed and sleep unblocked.");

            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error disposing NotificationsViewModel.");
            }

        }
    }
}
