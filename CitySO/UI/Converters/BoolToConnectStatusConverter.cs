using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CitySO.UI.Converters;

public class BoolToConnectStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected 
                ? new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)) // Green for connected
                : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // Red for disconnected
        }
        return new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)); // Gray as default
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
