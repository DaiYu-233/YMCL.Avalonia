using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Module.App;

public class InitData
{
    public static void GetSettingData()
    {
        Data.Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(ConfigPath.SettingDataPath));
        Data.MinecraftFolders = JsonConvert.DeserializeObject<ObservableCollection<MinecraftFolder>>(File.ReadAllText(ConfigPath.MinecraftFolderDataPath));
        Data.JavaRuntimes = JsonConvert.DeserializeObject<ObservableCollection<JavaEntry>>(File.ReadAllText(ConfigPath.JavaDataPath));
        Data.Accounts = JsonConvert.DeserializeObject<ObservableCollection<AccountInfo>>(File.ReadAllText(ConfigPath.AccountDataPath));
    }
}