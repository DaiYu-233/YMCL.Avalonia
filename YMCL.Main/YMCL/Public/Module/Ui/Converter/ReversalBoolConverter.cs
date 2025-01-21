using System.Globalization;
using Avalonia.Data.Converters;

namespace YMCL.Public.Module.Ui.Converter;

public class ReversalBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(bool)value!;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(bool)value!;
    }
}