using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace YMCL.Public.Module.App;

public class AppMethod
{
    private static readonly Debouncer _debouncer = new(() =>
    {
        if (Data.Setting is null) return;
        File.WriteAllText(ConfigPath.SettingDataPath,
            JsonConvert.SerializeObject(Data.Setting, Formatting.Indented));
    }, 100);

    public static void SaveSetting()
    {
        _debouncer.Trigger();
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