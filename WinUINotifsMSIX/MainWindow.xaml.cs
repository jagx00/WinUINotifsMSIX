using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUINotifsMSIX
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private UserNotificationListener _listener;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNotificationListener();
        }

        private async void InitializeNotificationListener()
        {
            _listener = UserNotificationListener.Current;
            var accessStatus = await _listener.RequestAccessAsync();

            if (accessStatus == UserNotificationListenerAccessStatus.Allowed)
            {
                _listener.NotificationChanged += Listener_NotificationChanged;
            }
            else
            {
                // Handle access denied
            }
        }

        private void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            // Handle notification changed event
        }
    }
}
