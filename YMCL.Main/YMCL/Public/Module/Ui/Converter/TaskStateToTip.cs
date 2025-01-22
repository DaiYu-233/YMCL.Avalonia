using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Ui.Converter;

public class TaskStateToTip : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskState state)
        {
            return state switch
            {
                TaskState.Waiting => MainLang.Waiting,
                TaskState.Running => MainLang.Running,
                TaskState.Paused => MainLang.Paused,
                TaskState.Error => MainLang.Error,
                TaskState.Canceled => MainLang.Canceled,
                TaskState.Canceling => MainLang.Canceling,
                TaskState.Finished => MainLang.Finished,
                _ => state.ToString()
            };
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}