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
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using CrashWindow = YMCL.Views.CrashWindow;

namespace YMCL.Public.Module.App.Init.SubModule;

public class InitData
{
    public static void GetSettingData()
    {
        Data.MinecraftFolders =
            JsonConvert.DeserializeObject<ObservableCollection<MinecraftFolder>>(
                File.ReadAllText(ConfigPath.MinecraftFolderDataPath));
        Data.JavaRuntimes =
            JsonConvert.DeserializeObject<ObservableCollection<JavaEntry>>(
                File.ReadAllText(ConfigPath.JavaDataPath));
        Data.FavouriteResources =
            JsonConvert.DeserializeObject<ObservableCollection<FavouriteResourceEntry>>(
                File.ReadAllText(ConfigPath.FavouriteResourceDataPath));
        Data.FavouriteMinecraft =
            JsonConvert.DeserializeObject<ObservableCollection<FavouriteMinecraftEntry>>(
                File.ReadAllText(ConfigPath.FavouriteMinecraftDataPath));
        Data.Accounts =
            JsonConvert.DeserializeObject<ObservableCollection<AccountInfo>>(
                File.ReadAllText(ConfigPath.AccountDataPath));
        Data.EnablePlugins =
            JsonConvert.DeserializeObject<ObservableCollection<string>>(
                File.ReadAllText(ConfigPath.PluginDataPath));
        Data.SettingEntry =
            JsonConvert.DeserializeObject<SettingEntry>(File.ReadAllText(ConfigPath.SettingDataPath));
        try
        {
            if (Data.SettingEntry.SelectedMinecraftId == "bedrock")
            {
                if (Data.DesktopType == DesktopRunnerType.Windows && Environment.OSVersion.Version.Major >= 10)
                {
                    Data.UiProperty.SelectedMinecraft = new MinecraftDataEntry(null, true, true)
                        { IsSettingVisible = false, Type = "bedrock" };
                }
            }
            else
            {
                var parser = new MinecraftParser(Data.SettingEntry.MinecraftFolder.Path);
                Data.UiProperty.SelectedMinecraft =
                    new MinecraftDataEntry(parser.GetMinecraft(Data.SettingEntry.SelectedMinecraftId));
            }
        }
        catch
        {
        }
    }

    public static void InitCollection()
    {
        if (!Data.JavaRuntimes.Contains(new JavaEntry
                { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "Auto" }))
        {
            Data.JavaRuntimes.Insert(0,
                new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "Auto" });
        }
        else
        {
            Data.JavaRuntimes.FirstOrDefault(java => java.JavaVersion == "Auto").JavaPath =
                MainLang.LetYMCLChooseJava;
        }
    }

    public static void VerifyData()
    {
        if (!Data.MinecraftFolders.Contains(Data.SettingEntry.MinecraftFolder))
        {
            Data.SettingEntry.MinecraftFolder = Data.MinecraftFolders[0];
        }

        if (!Data.Accounts.Contains(Data.SettingEntry.Account))
        {
            Data.SettingEntry.Account = Data.Accounts[0];
        }

        if (!Data.JavaRuntimes.Contains(Data.SettingEntry.Java))
        {
            Data.SettingEntry.Java = Data.JavaRuntimes[0];
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
        DownloadMirrorManager.MaxThread = Data.SettingEntry.MaxDownloadThread;
        DownloadMirrorManager.IsEnableMirror = Data.SettingEntry.DownloadSource == Enum.Setting.DownloadSource.BmclApi;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }

    public static void ClearTempFolder()
    {
        try
        {
            IO.Disk.Setter.ClearFolder(ConfigPath.TempFolderPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}