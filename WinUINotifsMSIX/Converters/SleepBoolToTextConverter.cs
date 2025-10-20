using System;
using Microsoft.UI.Xaml.Data;

namespace WinUINotifsMSIX.Converters
{
    // _isSleepBlocked - this is no longer used , but kept for reference
    public class SleepBoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return b ? "Sleep Blocked" : "Sleep Allowed";
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }
}