using System.Globalization;
using Avalonia.Data.Converters;

namespace YMCL.Public.Module.Ui.Converter;

public class MaxThreadOverSafeConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue > 128;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;
    }
}