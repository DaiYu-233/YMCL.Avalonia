using YMCL.Public.Module.App.SubModule;
using String = System.String;

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
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
        Public.Module.Ui.Special.AggregateSearchUi.Filter(String.Empty);
    }

    public static void OnMainViewLoaded()
    {
        InitUi.Dispatch();
        YMCL.Public.Module.App.SubModule.NetAndUiLoader.Dispatcher.Dispatch();
    }
}