using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
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
            Data.Setting =
                JsonConvert.DeserializeObject<Setting>(await File.ReadAllTextAsync(ConfigPath.SettingDataPath));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            try
            {
                var win = new CrashWindow(e.ToString()!);
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
    }

    public static void InitCollection()
    {
        if (!Data.JavaRuntimes.Contains(new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "Auto" }))
        {
            Data.JavaRuntimes.Insert(0, new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "Auto" });
        }
        else
        {
            Data.JavaRuntimes.FirstOrDefault(java => java.JavaVersion == "Auto").JavaPath = MainLang.LetYMCLChooseJava;
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
}