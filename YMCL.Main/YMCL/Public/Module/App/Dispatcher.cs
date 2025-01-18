using YMCL.Public.Const;

namespace YMCL.Public.Module.App;

public static class InitDispatcher
{
    public static void BeforeCreateUi()
    {
        InitConfig.Dispatch();
        InitData.GetSettingData();
        InitUi.Dispatch();
        InitLang.Dispatch();
    }
}