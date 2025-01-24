using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Controls;

namespace YMCL.Public.Module.Ui.Converter;

public class NoTaskIsVisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<TaskEntry> tasks)
        {
            return tasks.Count <= 0;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}