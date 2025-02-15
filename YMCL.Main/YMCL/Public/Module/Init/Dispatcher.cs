using System.Threading.Tasks;
using YMCL.Public.Module.Init.SubModule;
using YMCL.Public.Module.Init.SubModule.GetDataFromNetwork;
using String = System.String;

namespace YMCL.Public.Module.Init;

public static class InitDispatcher
{
    public static async Task<bool> BeforeCreateUi()
    {
        DetectPlatform.Main();
        InitData.InitSystemMaxMem();
        InitConfig.Dispatch();
        if(!await InitData.GetSettingData()) return false;
        InitLang.Dispatch();
        InitData.InitCollection();
        InitData.VerifyData();
        InitData.InitMl();
        TranslateToken.RefreshToken();
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
        Public.Module.Ui.Special.AggregateSearchUi.Filter(string.Empty);
        return true;
    }

    public static void OnMainViewLoaded()
    {
        _ = InitUi.Dispatch();
        Dispatcher.Dispatch();
        InitPlugin.Dispatch();
        SettingChanged.Binding();
    }
}