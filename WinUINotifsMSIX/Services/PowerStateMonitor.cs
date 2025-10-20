using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinUINotifsMSIX.ViewModels;

namespace WinUINotifsMSIX.Services
{
    public class PowerStateMonitor : ViewModelBase
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SetThreadExecutionState(uint esFlags);

        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        private bool _isSleepBlocked;
        public bool IsSleepBlocked
        {
            get => _isSleepBlocked;
            set
            {
                if (SetProperty(ref _isSleepBlocked, value))
                {
                    UpdateExecutionState();
                    Log.Information("Sleep prevention toggled: _isSleepBlocked is now {State}", value);
                }
            }
        }

        private void UpdateExecutionState()
        {
            if (_isSleepBlocked)
            {
                SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            }
            else
            {
                SetThreadExecutionState(ES_CONTINUOUS);
            }
        }
    }
}
