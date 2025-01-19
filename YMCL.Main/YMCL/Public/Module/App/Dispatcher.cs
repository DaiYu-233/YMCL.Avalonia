namespace YMCL.Public.Module.App;

public static class InitDispatcher
{
    public static void BeforeCreateUi()
    {
        InitConfig.Dispatch();
        InitData.GetSettingData();
        InitLang.Dispatch();
    }

    public static void OnMainViewLoaded()
    {
        InitUi.Dispatch();
    }
}