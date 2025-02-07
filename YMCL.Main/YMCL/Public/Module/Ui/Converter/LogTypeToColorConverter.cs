using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using YMCL.Public.Enum;

namespace YMCL.Public.Module.Ui.Converter;

public class LogTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogType logType)
            return new SolidColorBrush(Colors.Gray);

        return logType switch
        {
            LogType.Error => new SolidColorBrush(Color.Parse("#B71C1C")),
            LogType.Info => new SolidColorBrush(Color.Parse("#00695C")),
            LogType.Debug => new SolidColorBrush(Color.Parse("#455A64")),
            LogType.Fatal => new SolidColorBrush(Color.Parse("#BF360C")),
            LogType.Warning => new SolidColorBrush(Color.Parse("#FF6F00")),
            LogType.Exception => new SolidColorBrush(Color.Parse("#F26689")),
            LogType.StackTrace => new SolidColorBrush(Color.Parse("#795548")),
            LogType.Unknown => new SolidColorBrush(Color.Parse("#616161")),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}