using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Classes;

namespace YMCL.Public.Module.Ui.Converter;

public class SelectedGameIsBedrock: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GameDataEntry entry) return false;
        return entry.Entry.Type != "bedrock";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}