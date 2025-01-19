using YMCL.Public.Langs;

namespace YMCL.Public.Module.App;

public class InitLang
{
    public static void Dispatch()
    {
        if (Data.Instance.Setting.Language == null || Data.Instance.Setting.Language == "zh-CN")
            LangHelper.Current.ChangedCulture("");
        else
            LangHelper.Current.ChangedCulture(Data.Instance.Setting.Language);
    }
}