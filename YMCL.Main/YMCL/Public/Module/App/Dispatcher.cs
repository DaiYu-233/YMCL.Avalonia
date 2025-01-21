using YMCL.Public.Module.App.SubModule;

namespace YMCL.Public.Module.App;

public static class InitDispatcher
{
    public static void BeforeCreateUi()
    {
        DetectPlatform.Main();
        InitConfig.Dispatch();
        InitData.GetSettingData();
        InitData.VerifyData();
        InitLang.Dispatch();
    }

    public static void OnMainViewLoaded()
    {
        InitUi.Dispatch();
    }
}