using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.ViewModels;

namespace YMCL.Public.Module.Ui.Converter;

public class TaskStateToProgressConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TaskEntryModel entry) return 0;
        if (!entry.NumberValue)
        {
            return entry.State switch
            {
                TaskState.Finished => 100,
                TaskState.Error => 50,
                TaskState.Paused => 50,
                _ => entry.Value
            };
        }

        return entry.Value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}