using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Const;
using YMCL.Public.Enum;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Module.App;

public class InitData
{
    public static void GetSettingData()
    {
        Data.Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(ConfigPath.SettingDataPath));
        Data.MinecraftFolders = JsonConvert.DeserializeObject<ObservableCollection<MinecraftFolder>>(File.ReadAllText(ConfigPath.MinecraftFolderDataPath));
    }
}