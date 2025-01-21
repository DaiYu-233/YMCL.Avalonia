using System.Globalization;
using Avalonia.Data.Converters;

namespace YMCL.Public.Module.Ui.Converter;

public class MsToTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double ms)
        {
            return Value.Converter.MsToTime(ms);
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}