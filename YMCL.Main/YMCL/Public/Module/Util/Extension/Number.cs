namespace YMCL.Public.Module.Util.Extension;

public static class Number
{
    public static string ToUnit(this double number)
    {
        var isChinese = false;
        if (Data.SettingEntry != null)
            isChinese = Data.SettingEntry.Language.Code == "zh-CN";
        if (isChinese)
        {
            if (number >= 100000000)
            {
                return $"{number / 100000000:F2}亿";
            }

            if (number >= 10000)
            {
                return $"{number / 10000:F2}万";
            }
            return number >= 1000 ? $"{number / 1000:F2}千" : number.ToString("F2");
        }

        if (number >= 1000000000)
        {
            return $"{number / 1000000000:F2}b";
        }

        if (number >= 1000000)
        {
            return $"{number / 1000000:F2}m";
        }
        return number >= 1000 ? $"{number / 1000:F2}k" : number.ToString("F2");
    }
}