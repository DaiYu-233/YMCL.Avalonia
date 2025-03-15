using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using YMCL.Public.Langs;
using YMCL.Public.Module.App.Init.SubModule;
using YMCL.Public.Module.App.Init.SubModule.GetDataFromNetwork;
using YMCL.Views;

namespace YMCL.Public.Module.App.Init;

public static class InitDispatcher
{
    public static async Task<bool> BeforeCreateUi()
    {
        DetectPlatform.Main();
        InitData.InitSystemMaxMem();
        InitConfig.Dispatch();
        InitData.ClearTempFolder();
        if(!await ActionInvokeWithCrash(InitData.GetSettingData)) return false;
        InitLang.Dispatch();
        if(!await ActionInvokeWithCrash(InitData.InitCollection)) return false;
        if(!await ActionInvokeWithCrash(InitData.VerifyData)) return false;
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

    public static async Task<bool> ActionInvokeWithCrash(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            try
            {
                var win = new CrashWindow(e.ToString());
                win.Show();
                var dialog = await ShowDialogAsync(MainLang.ResetData, MainLang.FixLoadDataFailTip,
                    b_primary: MainLang.Ok, b_cancel: MainLang.Cancel, p_host: TopLevel.GetTopLevel(win));
                if (dialog == ContentDialogResult.Primary)
                {
                    IO.Disk.Setter.ClearFolder(ConfigPath.UserDataRootPath);
                    AppMethod.RestartApp();
                }
            }
            catch
            {
            }

            return false;
        }
        return true;
    }
}