using System.IO;
using Newtonsoft.Json;

namespace YMCL.Public.Module.App;

public class Method
{
    public static void SaveSetting()
    {
        File.WriteAllText(Const.ConfigPath.SettingDataPath,
            JsonConvert.SerializeObject(Const.Data.Setting, Formatting.Indented));
    }
}