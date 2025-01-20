using System.Globalization;
using Avalonia.Data.Converters;
using YMCL.Public.Classes;
using Setting = YMCL.Public.Enum.Setting;

namespace YMCL.Public.Module.Ui.Converter;

public class AccountTypeIsMicrosoftConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AccountInfo accountInfo)
        {
            return accountInfo.AccountType == Setting.AccountType.Microsoft;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;
    }
}