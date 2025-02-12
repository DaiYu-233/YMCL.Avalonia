using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Components.Provider;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Views.Crash;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Module.Init.SubModule;

public class InitData
{
    public static async Task<bool> GetSettingData()
    {
        try
        {
            Data.MinecraftFolders =
                JsonConvert.DeserializeObject<ObservableCollection<MinecraftFolder>>(
                    await File.ReadAllTextAsync(ConfigPath.MinecraftFolderDataPath));
            Data.JavaRuntimes =
                JsonConvert.DeserializeObject<ObservableCollection<JavaEntry>>(
                    await File.ReadAllTextAsync(ConfigPath.JavaDataPath));
            Data.Accounts =
                JsonConvert.DeserializeObject<ObservableCollection<AccountInfo>>(
                    await File.ReadAllTextAsync(ConfigPath.AccountDataPath));
            Data.EnablePlugins =
                JsonConvert.DeserializeObject<ObservableCollection<string>>(
                    await File.ReadAllTextAsync(ConfigPath.PluginDataPath));
            Data.Setting =
                JsonConvert.DeserializeObject<Setting>(await File.ReadAllTextAsync(ConfigPath.SettingDataPath));
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

        try
        {
            if (Data.Setting.SelectedMinecraftId == "bedrock")
            {
                Data.UiProperty.SelectedMinecraft = new MinecraftDataEntry(null, true, true)
                    { IsSettingVisible = false, Type = "bedrock" };
            }
            else
            {
                var parser = new MinecraftParser(Data.Setting.MinecraftFolder.Path);
                Data.UiProperty.SelectedMinecraft = new MinecraftDataEntry(parser.GetMinecraft(Data.Setting.SelectedMinecraftId));
            }
        }
        catch
        {
        }

        return true;
    }

    public static void InitCollection()
    {
        if (!Data.JavaRuntimes.Contains(new JavaEntry
                { JavaPath = MainLang.LetYMCLChooseJava, JavaStringVersion = "Auto" }))
        {
            Data.JavaRuntimes.Insert(0,
                new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaStringVersion = "Auto" });
        }
        else
        {
            Data.JavaRuntimes.FirstOrDefault(java => java.JavaStringVersion == "Auto").JavaPath =
                MainLang.LetYMCLChooseJava;
        }
    }

    public static void VerifyData()
    {
        if (!Data.MinecraftFolders.Contains(Data.Setting.MinecraftFolder))
        {
            Data.Setting.MinecraftFolder = Data.MinecraftFolders[0];
        }
    }

    public static void InitSystemMaxMem()
    {
        var totalMemory = Public.Module.IO.Disk.Getter.GetTotalMemory(Data.DesktopType);
        if (totalMemory != 0)
            UiProperty.Instance.SystemMaxMem = Math.Floor(totalMemory / 1024);
        else
            UiProperty.Instance.SystemMaxMem = 65536;
    }
    
    public static void InitMl()
    {
        HttpUtil.Initialize();
        CurseforgeProvider.CurseforgeApiKey = Const.String.CurseForgeApiKey;
        DownloadMirrorManager.MaxThread = Data.Setting.MaxDownloadThread;
        DownloadMirrorManager.IsEnableMirror = Data.Setting.DownloadSource == Enum.Setting.DownloadSource.BmclApi;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }
}