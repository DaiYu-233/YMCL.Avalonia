using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.ViewModels;

namespace YMCL.Public.Module.Ui.Converter;

public class TaskIsIndeterminateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskEntryModel entry) return true;
        

        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}