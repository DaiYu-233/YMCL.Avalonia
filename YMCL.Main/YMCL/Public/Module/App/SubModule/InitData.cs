using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Module.App.SubModule;

public class InitData
{
    public static void GetSettingData()
    {
        Data.MinecraftFolders =
            JsonConvert.DeserializeObject<ObservableCollection<MinecraftFolder>>(
                File.ReadAllText(ConfigPath.MinecraftFolderDataPath));
        Data.JavaRuntimes =
            JsonConvert.DeserializeObject<ObservableCollection<JavaEntry>>(File.ReadAllText(ConfigPath.JavaDataPath));
        Data.Accounts =
            JsonConvert.DeserializeObject<ObservableCollection<AccountInfo>>(File.ReadAllText(ConfigPath.AccountDataPath));
        Data.Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(ConfigPath.SettingDataPath));
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