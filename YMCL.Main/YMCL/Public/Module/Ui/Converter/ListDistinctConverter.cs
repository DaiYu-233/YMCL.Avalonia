using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace YMCL.Public.Module.Ui.Converter;

public class ListDistinctConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var list = value as IEnumerable<object>;
        return list?.Distinct().ToList();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var list = value as IEnumerable<object>;
        return list?.Distinct().ToList();
    }
}