using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace YMCL.Public.Module.App;

public class Method
{
    public static void SaveSetting()
    {
        if (Data.Instance.Setting is null) return;
        File.WriteAllText(ConfigPath.SettingDataPath,
            JsonConvert.SerializeObject(Data.Instance.Setting, Formatting.Indented));
    }

    public static void RestartApp()
    {
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Process.GetCurrentProcess().MainModule.FileName
        };
        Process.Start(startInfo);
        Environment.Exit(0);
    }
}