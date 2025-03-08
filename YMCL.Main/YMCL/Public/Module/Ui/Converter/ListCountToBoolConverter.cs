using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;

namespace YMCL.Public.Module.Ui.Converter;

public class ListCountToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable list)
        {
            var b = list.Count() > 0;
            return b;
        }

        if (value is ObservableCollection<object> collection)
        {
            return collection.Count > 0;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}