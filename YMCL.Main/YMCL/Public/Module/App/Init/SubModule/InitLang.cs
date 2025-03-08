using YMCL.Public.Langs;

namespace YMCL.Public.Module.App.Init.SubModule;

public class InitLang
{
    public static void Dispatch()
    {
        if (Data.SettingEntry == null || Data.SettingEntry.Language == null || Data.SettingEntry.Language.Code == "zh-CN")
            LangHelper.Current.ChangedCulture("");
        else
            LangHelper.Current.ChangedCulture(Data.SettingEntry.Language.Code);
    }
}