using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Op;

public class JavaRuntime
{
    public static async void AddByAutoScan()
    {
        var repeatJavaCount = 0;
        var successAddCount = 0;
        var javaList = await JavaUtil.EnumerableJavaAsync().ToListAsync();
        var convertedJavaList = javaList.Select(JavaEntry.MlToYmcl).ToList();

        convertedJavaList.ForEach(java =>
        {
            if (!Data.JavaRuntimes.Contains(java))
            {
                Data.JavaRuntimes.Add(java);
                successAddCount++;
            }
            else
            {
                repeatJavaCount++;
            }
        });

        Toast(
            $"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}",
            NotificationType.Success);
        File.WriteAllText(ConfigPath.JavaDataPath, JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }

    public static async Task AddByUi(Control sender)
    {
        var list = await TopLevel.GetTopLevel(sender).StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { AllowMultiple = true, Title = MainLang.SelectJava });
        if (list.Count == 0 || string.IsNullOrWhiteSpace(list[0].Path.LocalPath)) return;
        MinecraftLaunch.Base.Models.Game.JavaEntry javaInfo = null;
        try
        {
            if (Data.DesktopType is DesktopRunnerType.Linux or DesktopRunnerType.MacOs)
            {
                // 给Java赋予执行权限 (Linux和MacOs) 通过命令行 chmod +777
                await Task.Run(() =>
                {
                    var process = new Process();
                    process.StartInfo.FileName = "chmod";
                    process.StartInfo.Arguments = "+777 " + list[0].Path.LocalPath;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                });
            }
            javaInfo = await JavaUtil.GetJavaInfoAsync(list[0].Path.LocalPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Toast(MainLang.GetJavaInfoFail, NotificationType.Error);
        }
        if (javaInfo == null)
        {
            Toast(MainLang.GetJavaInfoFail, NotificationType.Error);
        }
        else
        {
            if (Data.JavaRuntimes.Contains(JavaEntry.MlToYmcl(javaInfo!)))
                Toast(MainLang.TheItemAlreadyExist, NotificationType.Error);
            else
                Data.JavaRuntimes.Add(JavaEntry.MlToYmcl(javaInfo!));
        }

        await File.WriteAllTextAsync(ConfigPath.JavaDataPath,
            JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }

    public static void RemoveSelected()
    {
        if (Data.Setting.Java == null || Data.Setting.Java.JavaStringVersion == "Auto") return;
        Data.JavaRuntimes.Remove(Data.Setting.Java);
        Data.Setting.Java = new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaStringVersion = "Auto" };
        File.WriteAllText(ConfigPath.JavaDataPath,
            JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }
}