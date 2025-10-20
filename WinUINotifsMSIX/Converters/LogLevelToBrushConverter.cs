using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
namespace WinUINotifsMSIX.Converters
{

    public class LogLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var level = value?.ToString()?.ToLowerInvariant();
            return level switch
            {
                "error" => new SolidColorBrush(Colors.Red),
                "warning" => new SolidColorBrush(Colors.Orange),
                "information" => new SolidColorBrush(Colors.Gray),
                "debug" => new SolidColorBrush(Colors.SteelBlue),
                "verbose" => new SolidColorBrush(Colors.DarkGreen),
                _ => new SolidColorBrush(Colors.Black)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}