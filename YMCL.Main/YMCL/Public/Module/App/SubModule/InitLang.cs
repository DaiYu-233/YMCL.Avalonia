using YMCL.Public.Langs;

namespace YMCL.Public.Module.App;

public class InitLang
{
    public static void Dispatch()
    {
        if (Const.Data.Setting.Language == null || Const.Data.Setting.Language == "zh-CN")
            LangHelper.Current.ChangedCulture("");
        else
            LangHelper.Current.ChangedCulture(Const.Data.Setting.Language);
    }
}