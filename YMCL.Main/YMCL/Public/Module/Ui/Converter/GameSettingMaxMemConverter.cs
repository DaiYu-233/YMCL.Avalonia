using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Ui.Converter;

public class GameSettingMaxMemConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double maxMem) return null;
        if (maxMem < 0)
        {
            return MainLang.UseGlobalSetting;
        }
        return Math.Round(maxMem);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}