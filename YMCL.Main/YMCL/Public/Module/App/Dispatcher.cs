using YMCL.Public.Module.App.SubModule;

namespace YMCL.Public.Module.App;

public static class InitDispatcher
{
    public static void BeforeCreateUi()
    {
        DetectPlatform.Main();
        InitData.InitSystemMaxMem();
        InitConfig.Dispatch();
        InitData.GetSettingData();
        InitLang.Dispatch();
        InitData.InitCollection();
        InitData.VerifyData();
    }

    public static void OnMainViewLoaded()
    {
        InitUi.Dispatch();
        YMCL.Public.Module.App.SubModule.NetAndUiLoader.Dispatcher.Dispatch();
    }
}