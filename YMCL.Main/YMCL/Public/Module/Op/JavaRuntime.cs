using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Public.Module.Op;

public class JavaRuntime
{
    public static async void AddByAutoScan()
    {
        try
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

            Notice($"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}",
                NotificationType.Success);
            await File.WriteAllTextAsync(ConfigPath.JavaDataPath,
                JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
            AggregateSearchUi.UpdateAllAggregateSearchEntries();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Notice(MainLang.OperateFailed , NotificationType.Error);
        }
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
            Notice(MainLang.GetJavaInfoFail, NotificationType.Error);
        }
        if (javaInfo == null)
        {
            Notice(MainLang.GetJavaInfoFail, NotificationType.Error);
        }
        else
        {
            if (Data.JavaRuntimes.Contains(JavaEntry.MlToYmcl(javaInfo!)))
                Notice(MainLang.TheItemAlreadyExist, NotificationType.Error);
            else
                Data.JavaRuntimes.Add(JavaEntry.MlToYmcl(javaInfo!));
        }

        await File.WriteAllTextAsync(ConfigPath.JavaDataPath,
            JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }

    public static void RemoveSelected()
    {
        if (Data.SettingEntry.Java == null || Data.SettingEntry.Java.JavaVersion == "Auto") return;
        Data.JavaRuntimes.Remove(Data.SettingEntry.Java);
        Data.SettingEntry.Java = new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "Auto" };
        File.WriteAllText(ConfigPath.JavaDataPath,
            JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }
}