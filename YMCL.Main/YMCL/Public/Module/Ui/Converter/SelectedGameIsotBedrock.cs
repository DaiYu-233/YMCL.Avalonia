using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Operate;

namespace YMCL.Public.Module.Ui.Converter;

public class SelectedGameIsotBedrock: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MinecraftDataEntry entry) return false;
        return entry.Type != "bedrock";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}