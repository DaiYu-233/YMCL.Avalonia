using System.Threading.Tasks;
using YMCL.Public.Module.App.Init.SubModule;
using YMCL.Public.Module.App.Init.SubModule.GetDataFromNetwork;

namespace YMCL.Public.Module.App.Init;

public static class InitDispatcher
{
    public static async Task<bool> BeforeCreateUi()
    {
        DetectPlatform.Main();
        InitData.InitSystemMaxMem();
        InitConfig.Dispatch();
        InitData.ClearTempFolder();
        if(!await InitData.GetSettingData()) return false;
        InitLang.Dispatch();
        InitData.InitCollection();
        InitData.VerifyData();
        InitData.InitMl();
        TranslateToken.RefreshToken();
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
        Public.Module.Ui.Special.AggregateSearchUi.Filter(string.Empty);
        Ui.Setter.ToggleTheme(Data.SettingEntry.Theme);
        return true;
    }

    public static void OnMainViewLoaded()
    {
        _ = InitUi.Dispatch();
        Dispatcher.Dispatch();
        InitPlugin.Dispatch();
        SettingChanged.Binding();
        Op.Parser.Handle(Public.Const.Data.AppArgs);
    }
}