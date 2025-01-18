using System.IO;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;

namespace YMCL.Views.Initialize.Pages;

public partial class JavaRuntime : UserControl
{
    public JavaRuntime()
    {
        InitializeComponent();
        BindingEvent();
        JavaRuntimeListBox.ItemsSource = Data.JavaRuntimes;
    }

    private void BindingEvent()
    {
        AutoScanJavaRuntimeBtn.Click += (_, _) =>
        {
            var fetcher = new JavaFetcher();
            var repeatJavaCount = 0;
            var successAddCount = 0;
            foreach (var java in fetcher.Fetch())
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
            }

            Toast($"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}",
                NotificationType.Success);
            File.WriteAllText(ConfigPath.JavaDataPath, JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
        };
        ManualAddJavaRuntimeBtn.Click += async (_, _) =>
        {
            var list = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions() { AllowMultiple = true, Title = MainLang.SelectJava });

            var javaInfo = JavaUtil.GetJavaInfo(list[0].Path.LocalPath);
            if (javaInfo == null && !string.IsNullOrWhiteSpace(list[0].Path.LocalPath))
            {
                Toast(MainLang.TheJavaIsError, NotificationType.Error);
            }
            else
            {
                if (Data.JavaRuntimes.Contains(javaInfo!))
                    Toast(MainLang.TheItemAlreadyExist ,NotificationType.Error);
                else
                    Data.JavaRuntimes.Add(javaInfo!);
            }
            await File.WriteAllTextAsync(ConfigPath.JavaDataPath, JsonConvert.SerializeObject(Data.JavaRuntimes, Formatting.Indented));
        };
    }
}