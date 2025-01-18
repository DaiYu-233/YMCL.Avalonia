using System.IO;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using Newtonsoft.Json;
using YMCL.Public.Const;
using YMCL.Public.Enum;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Module.App;

public class InitData
{
    public static void GetSettingData()
    {
        Data.Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(ConfigPath.SettingDataPath));
    }
}