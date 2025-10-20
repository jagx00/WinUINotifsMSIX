using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using WinUINotifsMSIX.ViewModels;

namespace WinUINotifsMSIX.Converters
{
    public class SelectionBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = value as NotificationItem;
            var selected = parameter as NotificationItem;

            if (item == null || selected == null)
                return new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            return item.ID == selected.ID
                ? new SolidColorBrush(Microsoft.UI.Colors.LightBlue)
                : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }
}