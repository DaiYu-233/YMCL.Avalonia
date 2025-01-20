using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Operate;

public class JavaRuntime
{
    public static void AddByAutoScan()
    {
        var fetcher = new JavaFetcher();
        var repeatJavaCount = 0;
        var successAddCount = 0;
        foreach (var java in fetcher.Fetch())
        {
            if (!Data.Instance.JavaRuntimes.Contains(java))
            {
                Data.Instance.JavaRuntimes.Add(java);
                successAddCount++;
            }
            else
            {
                repeatJavaCount++;
            }
        }

        Toast($"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}",
            NotificationType.Success);
        File.WriteAllText(ConfigPath.JavaDataPath, JsonConvert.SerializeObject(Data.Instance.JavaRuntimes, Formatting.Indented));
    }
    
    public static async Task AddByUi(Control sender)
    {
        var list = await TopLevel.GetTopLevel(sender).StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions() { AllowMultiple = true, Title = MainLang.SelectJava });
        if (list.Count == 0) return;
        var javaInfo = JavaUtil.GetJavaInfo(list[0].Path.LocalPath);
        if (javaInfo == null && !string.IsNullOrWhiteSpace(list[0].Path.LocalPath))
        {
            Toast(MainLang.TheJavaIsError, NotificationType.Error);
        }
        else
        {
            if (Data.Instance.JavaRuntimes.Contains(javaInfo!))
                Toast(MainLang.TheItemAlreadyExist ,NotificationType.Error);
            else
                Data.Instance.JavaRuntimes.Add(javaInfo!);
        }
        await File.WriteAllTextAsync(ConfigPath.JavaDataPath, JsonConvert.SerializeObject(Data.Instance.JavaRuntimes, Formatting.Indented));
    }

    public static void RemoveSelected()
    {
        if (Data.Instance.Setting.Java != null && Data.Instance.Setting.Java.JavaPath != "All")
        {
            Data.Instance.JavaRuntimes.Remove(Data.Instance.Setting.Java);
            File.WriteAllText(ConfigPath.JavaDataPath,
                JsonConvert.SerializeObject(Data.Instance.JavaRuntimes, Formatting.Indented));
        }
    }
}