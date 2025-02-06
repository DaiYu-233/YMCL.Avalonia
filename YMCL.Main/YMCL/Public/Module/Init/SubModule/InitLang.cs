using YMCL.Public.Langs;

namespace YMCL.Public.Module.Init.SubModule;

public class InitLang
{
    public static void Dispatch()
    {
        if (Data.Setting == null || Data.Setting.Language == null || Data.Setting.Language.Code == "zh-CN")
            LangHelper.Current.ChangedCulture("");
        else
            LangHelper.Current.ChangedCulture(Data.Setting.Language.Code);
    }
}