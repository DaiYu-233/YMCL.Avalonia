using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Resolver;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using Setting = YMCL.Public.Enum.Setting;

namespace YMCL.Public.Module.Mc.Installer.InstallJavaClientByMinecraftLauncher;

public class Vanllia
{
    public static async Task<bool> Install(string id, TaskEntry task, SubTask checkSubTask = null,
        SubTask downloadSubTask = null, CancellationToken cancellationToken = default)
    {
        var shouldReturn = false;
        var resolver = new GameResolver(Data.Setting.MinecraftFolder.Path);
        var vanlliaInstaller = new VanlliaInstaller(resolver, id,
            new DownloaderConfiguration
            {
                MaxThread = Data.Setting.MaximumDownloadThread,
                IsEnableFragmentedDownload = Data.Setting.DownloadSource != Setting.DownloadSource.Mojang
            });
        await Task.Run(async () =>
        {
            try
            {
                vanlliaInstaller.ProgressChanged += (_, x) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.UpdateValue(x.Progress * 100);
                        ParseOutputAndHandle(x.ProgressStatus, checkSubTask, downloadSubTask);
                    });
                };

                var result = await vanlliaInstaller.InstallAsync(cancellationToken);

                if (!result)
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Toast($"{MainLang.InstallFail}: Vanllia - {id}", NotificationType.Error);
                    });
                    shouldReturn = true;
                }
            }
            catch (OperationCanceledException)
            {
                task.CancelFinish();
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowShortException($"{MainLang.InstallFail}: Vanllia - {id}", ex);
                });
                task.FinishWithError();
                shouldReturn = true;
            }
        });
        return !shouldReturn;
    }

    static void ParseOutputAndHandle(string output, SubTask checkSubTask, SubTask downloadSubTask)
    {
        if (output.Contains("Start downloading dependent resources"))
        {
            checkSubTask.Finish();
            downloadSubTask.State = TaskState.Running;
        }
        else if (output.StartsWith("Downloading dependent resources："))
        {
            var parts = output.Split("：");
            if (parts.Length != 2) return;
            var progress = parts[1].Split('/');
            if (progress.Length != 2 || !int.TryParse(progress[0], out var current) ||
                !int.TryParse(progress[1], out var total)) return;
            downloadSubTask.FinishedTask = current;
            downloadSubTask.TotalTask = total;
        }
    }
}