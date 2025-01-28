using YMCL.Public.Module.Init.SubModule;
using YMCL.Public.Module.Init.SubModule.NetAndUiLoader;
using String = System.String;

namespace YMCL.Public.Module.Init;

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
        TranslateToken.RefreshToken();
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
        Public.Module.Ui.Special.AggregateSearchUi.Filter(String.Empty);
    }

    public static void OnMainViewLoaded()
    {
        InitUi.Dispatch();
        Dispatcher.Dispatch();
    }
}