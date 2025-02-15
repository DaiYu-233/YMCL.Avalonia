using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace YMCL.Public.Module.Ui.Converter;

public class SpecialControlEnableListConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return str.Split(',').Select(x => x.Trim()).Distinct().ToList();
        }
        return new List<string>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<object> items)
        {
            var validItems = items.OfType<string>().Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(",", validItems.Distinct());
        }
        return string.Empty;
    }
}