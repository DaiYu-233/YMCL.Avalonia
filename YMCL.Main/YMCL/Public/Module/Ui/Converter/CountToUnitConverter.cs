using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Module.Util.Extension;

namespace YMCL.Public.Module.Ui.Converter;

public class CountToUnitConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return System.Convert.ToDouble(value).ToUnit();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}